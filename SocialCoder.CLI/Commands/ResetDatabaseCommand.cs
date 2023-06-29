using Spectre.Console;

namespace SocialCoder.CLI.Commands;

public class ResetDatabaseCommand : StartDatabaseCommand
{
    public ResetDatabaseCommand()
    {
        EntryText = "Resetting database...";
    }
    
    protected override void Run(SocialCoderDatabaseSettings settings)
    {
        var folder = Util.GetProjectPath();
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
        Util.ResetDb(folder, "social-coder-api-db", folder, settings.DockerImage, envFile, settings.PersistentPath);
#pragma warning restore CS8604
    }

}