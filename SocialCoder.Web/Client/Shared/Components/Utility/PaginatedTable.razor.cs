using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SocialCoder.Web.Client.Models;
using SocialCoder.Web.Shared.Requests;
using StrawberryShake;

namespace SocialCoder.Web.Client.Shared.Components.Utility;

/// <summary>
///     <para>
///         Reusable pagination component. Where the pagination is handled for you, you - the developer - can
///         focus purely on the content!
///     </para>
///     <para>
///         <see cref="MudTable{T}"/> pagination takes a data source on the client and paginates that.
///         This isn't ideal for scenarios where we want to paginate data from our database. Clearly
///         we don't want to pull 100 items down if we only want to view 20 at a time. Unless we have +/- 20 around
///         current location for "caching" purposes.
///     </para>
///     <para>
///         Have kept the same naming conventions as <see cref="MudTable{T}"/> to help prevent confusion when
///         implementing things. The main difference is the requirement in providing a <see cref="FetchDataFunc"/>
///         which is utilized to retrieve data from the database.
///     </para> 
/// </summary>
/// <typeparam name="TItem">Type of records that shall get displayed on each row</typeparam>
public partial class PaginatedTable<TItem>
{
    /// <summary>
    /// Utilize to track stored settings for descending / page size
    /// </summary>
    [Inject]
    protected ILocalStorageService Storage { get; set; }


    [Parameter] public int PageSize { get; set; }
    [Parameter] public int PageNumber { get; set; }
    
    /// <summary>
    /// Invoked whenever the user changes page
    /// </summary>
    [Parameter] 
    public EventCallback OnPageChange { get; set; }

    /// <summary>
    /// Content that gets passed to <see cref="MudTable{T}"/> for row generation
    /// </summary>
    [Parameter] 
    public RenderFragment<TItem>? RowTemplate { get; set; }
    
    /// <summary>
    /// Content that gets passed to <see cref="MudTable{T}"/> as column headers
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// When an error occurs when interacting with GraphQL, this fragment can be used to help display those errors.
    /// </summary>
    [Parameter] 
    public RenderFragment<IClientError>? ErrorContent { get; set; }

    /// <summary>
    /// Accessibility to theme colors
    /// </summary>
    private MudTheme Theme { get; set; } = new();

    private QueryResponse<TItem>? _items;

    /// <summary>
    /// Boolean value that can be utilized for when data is being fetched
    /// </summary>
    private bool _isFetching;

    /// <summary>
    /// Function utilized when fetching data
    /// </summary>
    [Parameter] 
    public Func<PageInfo, Task<QueryResponse<TItem>>> FetchDataFunc { get; set; }

    /// <summary>
    /// Are there any pages before the one we're on right now?
    /// </summary>
    private bool HasPrevious => PageNumber > 1;

    private int TotalPages => (int)Math.Ceiling((double)(_items?.TotalDbCount ?? 1) / PageSize);
    
    /// <summary>
    /// Are there any pages after the one we're on right now?
    /// </summary>
    private bool HasNext => PageNumber < TotalPages;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (PageSize <= 0) // trying not to have an empty data set here
        {
            if (await Storage.ContainKeyAsync("PageSize"))
                PageSize = await Storage.GetItemAsync<int>("PageSize");
            else
                PageSize = 10;
        }

        if (PageNumber <= 0)
            PageNumber = 1;
        
        await ReRender();
    }
    
    /// <summary>
    /// Toggles fetching state before/after calling <see cref="FetchDataFunc"/>.
    /// Then invokes StateHasChanged to rerender changes
    /// </summary>
    private async Task ReRender()
    {
        _isFetching = true;
        _items = await FetchDataFunc(new(PageSize, (PageNumber-1)*PageSize));
        _isFetching = false;

        StateHasChanged();
    }

    #region Page Change
    /// <summary>
    /// Increment current page number by 1
    /// </summary>
    protected virtual async Task NextPage()
    {
        if (PageNumber + 1 > TotalPages)
            return;

        PageNumber++;
        await ReRender();
        
        if(OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }

    /// <summary>
    /// Decrement current page number by one
    /// </summary>
    protected virtual async Task PreviousPage()
    {
        if (PageNumber <= 1)
            return;

        PageNumber--;
        await ReRender();
        
        if(OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }
    
    /// <summary>
    /// Move to the last page of the paginated series
    /// </summary>
    protected virtual async Task ToLastPage()
    {
        if (PageNumber >= TotalPages)
            return;
        
        PageNumber = TotalPages;
        await ReRender();
        
        if(OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }

    /// <summary>
    /// Move to first page of paginated series
    /// </summary>
    protected virtual async Task ToFirstPage()
    {
        if (PageNumber <= 1)
            return;

        PageNumber = 1;
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
        await Storage.SetItemAsync("PageSize", PageSize);
        await ReRender();

        if (OnPageChange.HasDelegate)
            await OnPageChange.InvokeAsync();
    }
}