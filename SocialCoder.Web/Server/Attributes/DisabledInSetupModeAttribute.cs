namespace SocialCoder.Web.Server.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DisabledInSetupModeAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class OnlyInSetupModeAttribute : Attribute;