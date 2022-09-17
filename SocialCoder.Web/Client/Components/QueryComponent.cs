using Microsoft.AspNetCore.Components;

namespace SocialCoder.Web.Client.Components;

/// <summary>
/// Component which provides basic injected services for use with interacting with <see cref="SocialCoderGraphQLClient"/>
/// </summary>
public abstract class QueryComponent : ComponentBase
{
    [Inject] protected SocialCoderGraphQLClient GraphQlClient { get; set; }
    [Inject] protected ILogger<QueryComponent> Logger { get; set; }
}