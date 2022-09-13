using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace SocialCoder.Web.Client.Components;

/// <summary>
/// Component which provides basic injected services for use with interacting with <see cref="SocialCoderGraphQLClient"/>
/// </summary>
public abstract class QueryComponent : ComponentBase
{
    [Inject] protected ILocalStorageService Storage { get; set; }
    [Inject] protected SocialCoderGraphQLClient GraphQLClient { get; set; }
    [Inject] protected ILogger<QueryComponent> Logger { get; set; }

    protected string UserId { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UserId = await Storage.GetItemAsStringAsync(Constants.UserId);
    }
}