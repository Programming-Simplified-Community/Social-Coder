using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SocialCoder.CLI.Commands;

public class StartDatabaseCommand : Command
{
    string[] ValidateSettings(SocialCoderDatabaseSettings? settings)
    {
        List<string> errors = new();

        if (settings is null)
            errors.Add("[cyan]Database[/]: uninitialized settings");
        else
        {
            var properties = typeof(SocialCoderDatabaseSettings).GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(settings, null);

                if (prop.PropertyType != typeof(string)) 
                    continue;
                
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                    errors.Add($"[red]{prop.Name}[/] cannot be empty");
            }
        }
        
        return errors.Any() ? errors.ToArray() : Array.Empty<string>();
    }

    protected string EntryText { get; set; } = "Spooling up database...";

    protected virtual void Run(SocialCoderDatabaseSettings settings)
    {
        var folder = Util.GetProjectPath();
        var projectFolder = Path.Join(folder, "SocialCoder.Web", "Server");
        var envFile = Util.GetEnvironmentPath();

        var invalid = false;
        if (string.IsNullOrWhiteSpace(folder))
        {
            AnsiConsole.MarkupLine("[red]Project Folder[/] is invalid");
            invalid = true;
        }

        if (string.IsNullOrWhiteSpace(envFile))
        {
            AnsiConsole.MarkupLine("[red]Env File[/] is invalid");
            invalid = true;
        }

        if (invalid)
            return;
        
        // checks are already done above, so this is safe
#pragma warning disable CS8604
        Util.StartDb(folder, "social-coder-api-db", projectFolder, settings.DockerImage, envFile);
#pragma warning restore CS8604
    }
    
    public override int Execute(CommandContext context)
    {
        var config = Util.GetDbSettings();

        var errors = ValidateSettings(config);

        if (errors.Any())
        {
            AnsiConsole.MarkupLine(string.Join("\n", errors));
            return 1;
        }
        
        AnsiConsole.MarkupLine(EntryText);
        Run(config);

        return 0;
    }
}