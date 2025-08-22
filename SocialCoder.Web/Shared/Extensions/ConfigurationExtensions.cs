using Microsoft.Extensions.Configuration;

namespace SocialCoder.Web.Shared.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Loads all configuration files at <paramref name="folderPath"/> (if specified) or <see cref="AppDomain.CurrentDomain.BaseDirectory"/> that
    /// have the '.dev' or '.development' file extension.
    /// </summary>
    /// <example>
    ///     <list type="bullet">
    ///         <item>appsettings.dev.json</item>
    ///         <item>appsettings.development.json</item>
    ///         <item>appsettings.development</item>
    ///     </list>
    /// </example>
    /// <param name="builder"></param>
    /// <param name="folderPath"></param>
    /// <returns><paramref name="builder"/> to allow chained calls</returns>
    /// <exception cref="DirectoryNotFoundException">When <paramref name="folderPath"/> is not found</exception>
    public static IConfigurationBuilder AddJsonConfigurationFiles(this IConfigurationBuilder builder, string? folderPath = null)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            folderPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException(folderPath);
        }

        var files = Directory.GetFiles(folderPath, "*.json")
            .Where(x => x.Contains(".dev", StringComparison.InvariantCultureIgnoreCase));

        foreach (var configFile in files)
        {
            Console.WriteLine($"Adding {configFile} to configuration");
            builder.AddJsonFile(configFile, optional: true, reloadOnChange: true);
        }

        return builder;
    }

    /// <summary>
    /// Add a connections environment file as InMemory Key Value Pairs
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="connectionsFileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static IConfigurationBuilder AddConnections(this IConfigurationBuilder builder,
        string connectionsFileName = ".connections")
    {
        var connectionsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, connectionsFileName);

        if (!File.Exists(connectionsPath))
        {
            return builder;
        }

        Dictionary<string, string> vars = new();
        foreach (var line in File.ReadAllLines(connectionsPath))
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // We need to grab up to the FIRST equals sign
            var equalsIndex = line.IndexOf('=');

            // Not found
            if (equalsIndex < 0)
            {
                continue;
            }

            var key = line.Substring(0, equalsIndex);
            var value = line.Substring(equalsIndex + 1);

#if DEBUG
            Console.WriteLine($"{key}: {value}");
#endif

            if (!vars.ContainsKey(key))
            {
                vars.Add(key, value);
            }
        }

        builder.AddInMemoryCollection(vars);

        return builder;
    }
}