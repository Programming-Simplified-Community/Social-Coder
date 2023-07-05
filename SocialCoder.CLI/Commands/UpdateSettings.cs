using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
// ReSharper disable InconsistentNaming

namespace SocialCoder.CLI.Commands;

public class UpdateSettings : InitializeSettings
{
    private const string PROJECT_PATH = "Project Path";
    private const string DATABASE_SETTINGS = "Database Settings";
    private const string CERT_SETTINGS = "SSL Cert Settings";
    

    public override int Execute(CommandContext context)
    {
        ProjectPath = Util.GetProjectPath();
        DbSettings = Util.GetDbSettings();
        CertSettings = Util.GetCertSettings();
        
        if (string.IsNullOrWhiteSpace(ProjectPath) || !Directory.Exists(ProjectPath))
        {
            AnsiConsole.MarkupLine($"[red]Invalid Project Path provided: {ProjectPath}[/]");
            return 1;
        }
        
        var options = new SelectionPrompt<string>()
            .Title("Which setting would you like to update?")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to select)[/]")
            .AddChoices(PROJECT_PATH, DATABASE_SETTINGS, CERT_SETTINGS);

        var option = AnsiConsole.Prompt(options);

        switch (option)
        {
            case PROJECT_PATH:
                CheckAndValidateProjectPath();
                break;
            case DATABASE_SETTINGS:
                DbSettings = Prompter.EditorFor(DbSettings);
                break;
            case CERT_SETTINGS:
                CertSettings = Prompter.EditorFor(CertSettings);
                break;
            default:
                throw new NotImplementedException(option);
        }
        
        Save();

        return 0;
    }
}