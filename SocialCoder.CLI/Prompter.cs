using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace SocialCoder.CLI;

public static class Prompter
{
    // ReSharper disable once InconsistentNaming
    private const string EXIT = "EXIT";
    private static Regex _regex = new(@"\[cyan\](.*?)\[/\]", RegexOptions.Compiled);
    
    record Prop(string EnvName, string PropName, string Description, PropertyInfo Property);

    static Dictionary<string, Prop> ExtractPropsFrom<T>()
    {
        Dictionary<string, Prop> props = new();

        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.SetMethod is null)
                continue;

            var envName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? prop.Name;
            var description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
            props.Add(envName, new(envName, prop.Name, description, prop));
        }

        return props;
    }

    public static T EditorFor<T>(T settings)
    {
        var className = typeof(T).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? typeof(T).Name;
        var props = ExtractPropsFrom<T>();

        var options = new List<string>();
        foreach (var prop in props)
            options.Add(string.IsNullOrEmpty(prop.Value.Description)
                ? $"[cyan]{prop.Key}[/]"
                : $"[cyan]{prop.Key}[/]\n\t[green]{prop.Value.Description}[/]\n\tCurrent Value: {prop.Value.Property.GetValue(settings,null)}");

        options.Add(EXIT);

        while (true)
        {
            var prompt = new SelectionPrompt<string>()
                .Title($"Editor for {className}")
                .PageSize(10)
                .AddChoices(options.ToArray());
            
            var option = AnsiConsole.Prompt(prompt);

            var match = _regex.Match(option);
            
            if (option == EXIT)
                break;
            
            var selection = props[match.Groups[^1].Value];
            var currentValue = selection.Property.GetValue(settings);

            if (selection.Property.PropertyType == typeof(string))
                HandleStringProperty(settings, selection, currentValue);
            else if (selection.Property.PropertyType == typeof(int))
                HandleIntProperty(settings, selection, currentValue);    
        }

        return settings;
    }

    private static void HandleIntProperty<T>(T instance, Prop prop, object? currentValue)
    {
        var value = AnsiConsole.Ask<int>($"Please provide a new value for [cyan]{prop.EnvName}[/]\n\tCurrent Value: [yellow]{currentValue}[/]: ");
        prop.Property.SetValue(instance, value);
    }

    private static void HandleStringProperty<T>(T instance, Prop prop, object? currentValue)
    {
        var value = AnsiConsole.Ask<string>($"Please provide a new value for [cyan]{prop.EnvName}[/]\n\tCurrent Value: [yellow]{currentValue}[/]: ");
        prop.Property.SetValue(instance, value);
    }
}