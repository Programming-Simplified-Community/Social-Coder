using Microsoft.Extensions.Configuration;

namespace SocialCoder.Web.Shared.Extensions;

public static class ConfigurationExtensions
{
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
        
        if(!File.Exists(connectionsPath))
            throw new FileNotFoundException("Was unable to locate `.connections` file");
        
        Dictionary<string, string> vars = new();
        foreach (var line in File.ReadAllLines(connectionsPath))
        {
            if (string.IsNullOrEmpty(line))
                continue;
    
            // We need to grab up to the FIRST equals sign
            var equalsIndex = line.IndexOf('=');

            // Not found
            if (equalsIndex < 0)
                continue;

            var key = line.Substring(0, equalsIndex);
            var value = line.Substring(equalsIndex + 1);
            
            #if DEBUG
            Console.WriteLine($"{key}: {value}");
            #endif
            
            if (!vars.ContainsKey(key))
                vars.Add(key, value);
        }

        builder.AddInMemoryCollection(vars);

        return builder;
    }
}