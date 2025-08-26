using SocialCoder.Web.Server.Services;

namespace SocialCoder.Web.Server.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddEncryptedSettings(this IConfigurationBuilder builder, IServiceCollection services)
    {
        return builder.Add(new EncryptedSettingsConfigurationSource(services));
    }
}