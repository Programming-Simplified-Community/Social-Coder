using System.Reflection;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SocialCoder.CLI.Commands;

public class InitializeSettings : Command
{
    protected string? ProjectPath;
    protected SocialCoderDatabaseSettings? DbSettings;
    protected SocialCoderCertSettings? CertSettings;
    
    protected void CheckAndValidateProjectPath()
    {
        const string promptText = "Where is Social-Coder located on disk? (Full Path)";
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("Social Coder Project Path")
                .Centered()
                .Color(Color.Cyan1));
        
        if (string.IsNullOrWhiteSpace(ProjectPath) || Util.AskToChange("Project Path", ProjectPath))
            ProjectPath = AnsiConsole.Ask<string>(promptText);

        // Directory must be valid
        while (string.IsNullOrWhiteSpace(ProjectPath) || !Directory.Exists(ProjectPath))
        {
            AnsiConsole.MarkupLine($"[red]Project Path[/]: Directory does not exist at [red]{ProjectPath}[/]");
            ProjectPath = AnsiConsole.Ask<string>(promptText);
        }
    }

    protected void CheckAndValidateDatabase()
    {
        var props = typeof(SocialCoderDatabaseSettings).GetProperties();
     
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine(DbSettings!.ToString());
        
        AnsiConsole.Write(new FigletText("Database Settings")
            .Centered()
            .Color(Color.Cyan1));
        
        foreach (var prop in props)
        {
            if (prop.SetMethod is null)
                continue;
            
            var value = prop.GetValue(DbSettings, null);

            if (prop.PropertyType == typeof(string))
                HandleStringProperty(value, prop);
            else if (prop.PropertyType == typeof(int))
                HandleIntProperty(value, prop);
        }
    }

    protected void CheckAndValidateCertSettings()
    {
        var props = typeof(SocialCoderCertSettings).GetProperties();

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(CertSettings!.ToString());        
        AnsiConsole.Write(new FigletText("SSL Settings")
            .Centered()
            .Color(Color.Green3));

        foreach (var prop in props)
        {
            if (prop.SetMethod is null)
                continue;

            var value = prop.GetValue(CertSettings, null);

            if (prop.PropertyType == typeof(string))
                HandleStringProperty(value, prop);
            else if (prop.PropertyType == typeof(int))
                HandleIntProperty(value, prop);
        }
    }

    private void HandleIntProperty(object? value, PropertyInfo prop)
    {
        var asNum = (int?)value ?? default(int);

        if (asNum != 0)
            return;
        
        asNum = AnsiConsole.Ask<int>($"What should [cyan]{prop.Name}[/] be?");
        prop.SetValue(DbSettings, asNum);
    }

    private void HandleStringProperty(object? value, PropertyInfo prop)
    {
        var asString = value?.ToString();

        if (!string.IsNullOrWhiteSpace(asString) && !Util.AskToChange(prop.Name, asString))
            return;

        value = AnsiConsole.Ask<string>($"What should [cyan]{prop.Name}[/] be?");
        prop.SetValue(DbSettings, value);
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    public record JsonSettings(string ProjectPath, SocialCoderDatabaseSettings Database, SocialCoderCertSettings CertSettings);

    protected void Save()
    {
        var settings = new JsonSettings(ProjectPath!, DbSettings!, CertSettings!);
        var jsonText = JsonConvert.SerializeObject(settings, Formatting.Indented);
                
        Util.SaveSettings(jsonText);
        Util.SaveEnvironmentFile(settings);
        Util.SaveConnections(DbSettings!);
    }
    
    
    public override int Execute(CommandContext context)
    {
        ProjectPath = Util.GetProjectPath();
        DbSettings = Util.GetDbSettings();
        CertSettings = Util.GetCertSettings();
        
        CheckAndValidateProjectPath();
        CheckAndValidateDatabase();
        CheckAndValidateCertSettings();

        if (string.IsNullOrWhiteSpace(ProjectPath) || !Directory.Exists(ProjectPath))
        {
            AnsiConsole.MarkupLine($"[red]Invalid Project Path provided: {ProjectPath}[/]");
            return 1;
        }
        
        Save();
        
        return 0;
    }
}