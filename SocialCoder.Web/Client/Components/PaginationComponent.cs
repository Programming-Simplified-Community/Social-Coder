using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SocialCoder.Web.Shared.Requests;

namespace SocialCoder.Web.Client.Components;

public abstract class PaginationComponent<TItem> : ComponentBase
{
    /// <summary>
    /// Utilize to track stored settings for descending / page size
    /// </summary>
    [Inject]
    protected ILocalStorageService Storage { get; set; }
    
    /// <summary>
    /// Settings that are used during the pagination request. Tracks the current page number, page size, and ascending/descending
    /// </summary>
    protected PaginationRequest PaginationSettings { get; private set; }

    /// <summary>
    /// Invoked whenever the user changes page
    /// </summary>
    [Parameter] 
    public EventCallback OnPageChange { get; set; }

    protected MudTheme Theme { get; set; } = new();

    protected PaginatedResponse<TItem> Items { get; set; }

    /// <summary>
    /// Boolean value that can be utilized for when data is being fetched
    /// </summary>
    protected bool IsFetching { get; set; }

    protected abstract Task<PaginatedResponse<TItem>> FetchData();
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var pageSize = 10;
        var isDescending = false;
        
        if (await Storage.ContainKeyAsync("PageSize"))
            pageSize = await Storage.GetItemAsync<int>("PageSize");

        if (await Storage.ContainKeyAsync("IsDescending"))
            isDescending = await Storage.GetItemAsync<bool>("IsDescending");

        PaginationSettings = new PaginationRequest
        {
            IsDescending = isDescending,
            PageNumber = 1,
            PageSize = pageSize
        };
        
        await ReRender();
    }

    protected bool HasPrevious => PaginationSettings.PageNumber > 1;
    protected bool HasNext => PaginationSettings.PageNumber < (Items?.TotalPages ?? 1);
    
    private async Task ReRender()
    {
        IsFetching = true;
        Items = await FetchData();
        IsFetching = false;

        StateHasChanged();
    }

    #region Page Change
    /// <summary>
    /// Increment current page number by 1
    /// </summary>
    protected virtual async Task NextPage()
    {
        if (PaginationSettings.PageNumber + 1 > Items.TotalPages)
            return;

        PaginationSettings.PageNumber++;
        await ReRender();
        
        if(OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }

    /// <summary>
    /// Decrement current page number by one
    /// </summary>
    protected virtual async Task PreviousPage()
    {
        if (PaginationSettings.PageNumber <= 1)
            return;

        PaginationSettings.PageNumber--;
        await ReRender();
        
        if(OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }
    
    /// <summary>
    /// Move to the last page of the paginated series
    /// </summary>
    protected virtual async Task ToLastPage()
    {
        if (PaginationSettings.PageNumber >= Items.TotalPages)
            return;
        
        PaginationSettings.PageNumber = Items.TotalPages;
        await ReRender();
        
        if(OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }

    /// <summary>
    /// Move to first page of paginated series
    /// </summary>
    protected virtual async Task ToFirstPage()
    {
        if (PaginationSettings.PageNumber <= 1)
            return;

        PaginationSettings.PageNumber = 1;
        await ReRender();

        if (OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }
    #endregion

    /// <summary>
    /// Callback for when the page size value has changed
    /// </summary>
    protected virtual async Task OnPageSizeChanged()
    {
        await Storage.SetItemAsync("PageSize", PaginationSettings.PageSize);
        await ReRender();

        if (OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }

    /// <summary>
    /// Callback for when the descending option has changed
    /// </summary>
    protected virtual async Task OnDescendingChanged()
    {
        await Storage.SetItemAsync("IsDescending", PaginationSettings.IsDescending);
        await ReRender();
        
        if (OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }
}