using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using SocialCoder.CLI.Commands;
using Spectre.Console;

namespace SocialCoder.CLI;

public static class Util
{
    /// <summary>
    /// Path to settings file
    /// </summary>
    private static string SettingsPath { get; } = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    private static DockerClient Docker { get; } = new DockerClientConfiguration().CreateClient();
    private static IConfiguration? _cachedConfig;
    

    public static void SaveSettings(string json)
    {
        File.WriteAllText(SettingsPath, json);
        _cachedConfig = null!;
    }

    /// <summary>
    /// Save the provided <paramref name="dbSettings"/> into an environment file located at <see cref="GetEnvironmentPath"/>
    /// </summary>
    /// <param name="dbSettings"></param>
    /// <param name="certSettings"></param>
    public static void SaveEnvironmentFile(InitializeSettings.JsonSettings settings)
    {
        var envPath = GetEnvironmentPath();

        if (string.IsNullOrWhiteSpace(envPath))
        {
            AnsiConsole.MarkupLine("[red]Environment Path unknown/not set[/]");
            return;
        }
        
        using var writer = new StreamWriter(File.OpenWrite(envPath));
        
        foreach (var prop in typeof(SocialCoderDatabaseSettings).GetProperties())
        {
            var value = prop.GetValue(settings.Database, null);

            var nameAttribute = prop.GetCustomAttribute<DisplayNameAttribute>();

            if (nameAttribute is not null)
                writer.WriteLine($"{nameAttribute.DisplayName}={value}");
        }

        foreach (var prop in typeof(SocialCoderCertSettings).GetProperties())
        {
            var value = prop.GetValue(settings.CertSettings, null);

            var nameAttribute = prop.GetCustomAttribute<DisplayNameAttribute>();

            if (nameAttribute is not null)
                writer.WriteLine($"{nameAttribute.DisplayName}={value}");
        }
        
        writer.WriteLine($"ProjectPath={settings.ProjectPath}");
    }

    /// <summary>
    /// Save connection string associated with values in <paramref name="settings"/> to environment file located at
    /// <see cref="GetEnvironmentPath"/>
    /// </summary>
    /// <param name="settings"></param>
    public static void SaveConnections(SocialCoderDatabaseSettings settings)
    {
        var path = GetEnvironmentPath();

        if (string.IsNullOrWhiteSpace(path))
            return;
        
        var fileInfo = new FileInfo(path);
        var connectionPath = Path.Join(fileInfo.DirectoryName, ".connections");

        var connection = $"Server={settings.Host};port=5432;User Id={settings.User};Password={settings.Password};Database={settings.Name}";
        File.WriteAllText(connectionPath, connection);
    }
    
    /// <summary>
    /// Retrieve configuration from aggregate sources.
    ///
    /// <list type="number">
    ///     <item>Json File</item>
    ///     <item>Environment variables (which may overwrite values from JSON</item>
    /// </list>
    /// </summary>
    /// <returns>Aggregated config values</returns>
    private static IConfiguration GetConfiguration()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_cachedConfig is not null)
            return _cachedConfig;

        _cachedConfig = new ConfigurationBuilder()
            .AddJsonFile(SettingsPath, optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        return _cachedConfig;
    }
    
    /// <summary>
    /// Extract database settings from <see cref="IConfiguration"/>
    /// </summary>
    /// <returns>Database settings</returns>
    public static SocialCoderDatabaseSettings GetDbSettings()
    {
        var settings = new SocialCoderDatabaseSettings();
        var config = GetConfiguration();

        config.Bind("Database", settings);
        return settings;
    }

    /// <summary>
    /// Extract SSL Settings from <see cref="IConfiguration"/>
    /// </summary>
    /// <returns>SSL Settings</returns>
    public static SocialCoderCertSettings GetCertSettings()
    {
        var settings = new SocialCoderCertSettings();
        var config = GetConfiguration();

        config.Bind("SSL", settings);
        return settings;
    }

    /// <summary>
    /// Retrieve path to Social Coder. This values resides in the <see cref="IConfiguration"/>, with key "ProjectPath"
    /// </summary>
    /// <returns>Path to Social Coder on filesystem (if provided)</returns>
    public static string? GetProjectPath()
        => GetConfiguration().GetValue<string>("ProjectPath");

    /// <summary>
    /// Path to store environment file
    /// </summary>
    /// <returns></returns>
    public static string? GetEnvironmentPath()
    {
        var path = GetProjectPath();

        if (string.IsNullOrWhiteSpace(path))
            return null;

        return Path.Join(path, ".env");
    }
    
    /// <summary>
    /// Executes command on host. Equivalent to python's "os.system" method
    /// </summary>
    /// <param name="tool">Tool that we're trying to utilize on the CLI</param>
    /// <param name="arguments">Any parameters we want to pass into given tool</param>
    /// <remarks>
    ///     This function relies on having the <paramref name="tool"/> in the environment PATH
    /// </remarks>
    private static void RunSystemCommand(string tool, string arguments)
    {
        ProcessStartInfo info = new(tool ,arguments);

        AnsiConsole.MarkupLine($"Executing the following:\n\t[cyan]{tool}[/] [yellow]{arguments}[/]");
        
        info.UseShellExecute = false;
        info.RedirectStandardOutput = true;

        var process = new Process();
        process.StartInfo = info;
        process.Start();
        process.WaitForExit();
    }

    private static void RunDockerCommand(string arguments)
    {
        RunSystemCommand("docker", arguments);
    }

    private static void RunDockerComposeCommand(string arguments)
    {
        RunSystemCommand("docker compose", arguments);
    }

    /// <summary>
    /// Block calling thread until the target container becomes healthy, or timeout occurs
    /// </summary>
    /// <param name="containerName">Name of container to search for</param>
    /// <param name="attempts">Maximum number of attempts/retries until we deem the container lost</param>
    /// <param name="checkInterval">Time in between checks</param>
    /// <returns>True if container is healthy, otherwise False</returns>
    private static bool CheckContainerHealth(string containerName, int attempts = 10, int checkInterval = 5)
    {
        while (attempts > 0)
        {
            AnsiConsole.MarkupLine($"Waiting for [cyan]{containerName}[/] to by healthy...");
            var allContainers = Docker.Containers.ListContainersAsync(new()
            {
                All = true
            }).GetAwaiter().GetResult();
            
            // The C# variant of docker appears to put a forward slash "/" in front of container name.
            // Not sure if it's consistent across operating systems?
            var container = allContainers.FirstOrDefault(x =>
                x.Names.First().Contains(containerName, StringComparison.InvariantCultureIgnoreCase));

            if (container is null)
                return false;

            if (container.Status.Contains("healthy", StringComparison.InvariantCultureIgnoreCase) ||
                container.State.Contains("healthy", StringComparison.InvariantCultureIgnoreCase))
                return true;
            
            // Wait period of time
            Task.Delay(checkInterval * 1000).GetAwaiter().GetResult();

            attempts--;
        }

        return false;
    }

    // ReSharper disable once InconsistentNaming
    private static void BuildSSLContainer()
    {
        var images = Docker.Images.ListImagesAsync(new()
        {
            All = true
        }).GetAwaiter().GetResult();

        // IF we already have an ssl container we'll just continue onwards here.
        if (images.Any(x => x.RepoTags.Any(y => y.Contains("sslcontainer"))))
            return;
        
        var folder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "SSL");
        var file = Path.Join(folder, "Dockerfile-ssl");
        
        var buildParams = new ImageBuildParameters();
        buildParams.Dockerfile = Path.Join(folder, "Dockerfile-ssl");
        buildParams.Tags = new List<string> { "sslcontainer" };
        
        RunDockerCommand($"build -t sslcontainer -f \"{file}\" {folder}");
    }

    // ReSharper disable once InconsistentNaming
    public static void CreateSSL()
    {
        BuildSSLContainer();
        var projectPath = GetProjectPath();
        var sslFolder = Path.Join(projectPath, "SSL");

        var sslSettings = GetCertSettings();
        var variables = new List<string>();

        // Dynamically piece together the environment variables
        foreach (var prop in sslSettings.GetType().GetProperties())
        {
            var envName = prop.GetCustomAttribute<DisplayNameAttribute>();

            if (envName is null)
                continue;

            var value = prop.GetValue(sslSettings, null);
            variables.Add($"-e {envName}=\"{value}\"");
        }
        
        RunDockerCommand($"run -v \"{sslFolder}:/certs\" {string.Join(" ", variables)} sslcontainer");
    }

    /// <summary>
    /// Ask user whether or not they wish to change a given value
    /// </summary>
    /// <param name="name">Key/Property Name</param>
    /// <param name="currentValue">Value currently associated to <paramref name="name"/></param>
    /// <returns>True if user wants to change, otherwise false.</returns>
    public static bool AskToChange(string name, string currentValue) 
        => AnsiConsole.Confirm($"[cyan]{name}[/]: [dodgerblue2]{currentValue}[/]. Do you wish to change?");

    /// <summary>
    /// Reset the Social Coder database. This includes:
    ///
    /// <list type="bullet">
    ///     <item>Tearing down social-coder-api-db</item>
    ///     <item>Deleting persistent data directory</item>
    ///     <item>Passes things off to <see cref="StartDb"/></item>
    /// </list>
    /// </summary>
    /// <param name="composeFolder">
    ///     <para>Folder in which the docker-compose file resides.</para>
    ///     <para>This is intended for future proofing. In the event we change/move the compose file elsewhere.
    ///           The project with migrations and the compose file are not the same
    ///     </para>
    /// </param>
    /// <param name="serviceName">Name of service to tear down</param>
    /// <param name="projectFolder">Folder which contains the database migrations</param>
    /// <param name="image">Docker image to utilize (ensures the latest version of it is on disk)</param>
    /// <param name="envPath">Path to environment file</param>
    /// <param name="persistentPath">Persistent path on host to mount database to</param>
    public static void ResetDb(
        string composeFolder,
        string serviceName,
        string projectFolder,
        string image,
        string envPath,
        string? persistentPath)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            AnsiConsole.MarkupLine($"[red]{nameof(image)}[/] cannot be empty");
            return;
        }

        string composeFilePath;

        if (File.Exists(Path.Join(composeFolder, "docker-compose.yml")))
            composeFilePath = Path.Join(composeFolder, "docker-compose.yml");
        else if (File.Exists(Path.Join(composeFolder, "compose.yml")))
            composeFilePath = Path.Join(composeFolder, "compose.yml");
        else
        {
            AnsiConsole.MarkupLine($"Unable to locate compose file for [cyan]{serviceName}[/] at [red]{composeFolder}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"Tearing down [cyan]{serviceName}[/]...");
        RunDockerComposeCommand($"--env-file \"{envPath}\" --file \"{composeFilePath}[/] rm -svf {serviceName}");

        if (Directory.Exists(persistentPath))
        {
            AnsiConsole.MarkupLine($"Deleting persistent db data at [cyan]{persistentPath}[/]");
            Directory.Delete(persistentPath, true);
        }
                
        StartDb(composeFolder, serviceName, projectFolder, image, envPath);
    }
    
    /// <summary>
    ///     Start the social-coder-api-db
    ///
    ///     <list type="bullet">
    ///         <item>Spooling up social-coder-api-db</item>
    ///         <item>If Healthy, will attempt to apply database migrations for a clean-slate</item>
    ///     </list>
    /// </summary>
    /// <param name="composeFolder">
    ///     <para>Folder in which the docker-compose file resides.</para>
    ///     <para>This is intended for future proofing. In the event we change/move the compose file elsewhere.
    ///           The project with migrations and the compose file are not the same
    ///     </para>
    /// </param>
    /// <param name="serviceName">Name of service to tear down</param>
    /// <param name="projectFolder">Folder which contains the database migrations</param>
    /// <param name="image">Docker image to utilize (ensures the latest version of it is on disk)</param>
    /// <param name="envPath">Path to environment file</param>
    public static void StartDb(
        string composeFolder, 
        string serviceName, 
        string projectFolder, 
        string image,
        string envPath)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            AnsiConsole.MarkupLine($"[red]{nameof(image)}[/] cannot be empty");
            return;
        }

        AnsiConsole.MarkupLine($"Pulling [green]{image}[/] for [cyan]{serviceName}[/]...");
        RunDockerCommand($"pull {image}");
        
        AnsiConsole.MarkupLine($"Starting [cyan]{serviceName}[/]");

        string composeFilePath;

        if (File.Exists(Path.Join(composeFolder, "docker-compose.yml")))
            composeFilePath = Path.Join(composeFolder, "docker-compose.yml");
        else if (File.Exists(Path.Join(composeFolder, "compose.yml")))
            composeFilePath = Path.Join(composeFolder, "compose.yml");
        else
        {
            AnsiConsole.MarkupLine($"Unable to locate compose file for [cyan]{serviceName}[/] at [red]{composeFolder}[/]");
            return;
        }
        
        RunDockerComposeCommand($"--file \"{composeFilePath}\" --env-file \"{envPath}\" up -d \"{serviceName}\"");

        var result = CheckContainerHealth(serviceName);
        if (!result)
        {
            AnsiConsole.MarkupLine($"Something is wrong with [red]{serviceName}[/]. Unable to perform migrations...");
            return;
        }

        AnsiConsole.MarkupLine($"Running migrations for [cyan]{serviceName}[/]...");
        RunSystemCommand("dotnet", $"ef database update --project \"{projectFolder}\"");
        AnsiConsole.MarkupLine($"Completed migrations for [cyan]{serviceName}[/]");
    }
}