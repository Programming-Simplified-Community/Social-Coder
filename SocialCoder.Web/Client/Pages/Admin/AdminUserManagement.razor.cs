using Microsoft.AspNetCore.Components;
using MudBlazor;
using SocialCoder.Web.Client.Components;
using SocialCoder.Web.Client.Models;
using SocialCoder.Web.Client.Shared.Components.Dialogs;
using SocialCoder.Web.Shared.Models.Account;
using StrawberryShake;

namespace SocialCoder.Web.Client.Pages.Admin;

public partial class AdminUserManagement : QueryComponent, IDisposable
{

    [Inject] private ISnackbar Snack { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    [CascadingParameter(Name="UserId")] public string UserId { get; set; }

    private int _pageSize = 25;
    private string? _beforeCursor;
    private string? _afterCursor;
    private BasicUserAccountInfoFilterInput? _filter;

    private BasicUserAccountInfoSortInput[] _sort = [new()
    {
        Username = SortEnumType.Asc
    }];

    private IDisposable? _onUserUpdatedSubscription;
    private IDisposable? _onUserDeletedSubscription;
    private IDisposable? _onUserBannedSubscription;

    private IList<CursorItem<UserAccountItem>> _users = new List<CursorItem<UserAccountItem>>();
    private HashSet<string> _roles = new();

    private DialogOptions _dialogOptions = new() { MaxWidth = MaxWidth.Large, FullScreen = true };

    private async Task BanUser(UserAccountItem user)
    {
        var dialog = await this.DialogService.ShowAsync<ReasonDialog>("Reason for Ban", this._dialogOptions);
        var result = await dialog.Result;

        if (result is null || result.Canceled)
        {
            return;
        }

        if (result.Data is null)
        {
            this.Snack.Add("Please enter a reason for the ban", Severity.Warning);
            return;
        }

        var response = await this.GraphQlClient.BanUser.ExecuteAsync(user.UserId, this.UserId, result.Data.ToString());

        if (response.IsErrorResult() || response.Data is null)
        {
            this.Snack.Add("Error with GraphQL", Severity.Error);
            return;
        }

        if (!response.Data.BanUser.Success)
        {
            this.Snack.Add(response.Data.BanUser.Message ?? "Error banning user", Severity.Error);
            return;
        }

        this.StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var result = await this.GraphQlClient.GetBasicUserAccounts
            .ExecuteAsync(
                this._pageSize,
                this._afterCursor,
                null,
                this._beforeCursor,
                this._sort,
                this._filter
            );

        if (result.Errors.Any())
        {
            this.Snack.Add("An error occurred", Severity.Error);
            await Console.Error.WriteLineAsync(string.Join("\n", result.Errors.Select(x => x.Message)));
            return;
        }

        if (result.Data?.Users?.Edges is null)
        {
            this.Snack.Add("No users found", Severity.Warning);
            return;
        }

        if (result.Data?.Roles is not null)
        {
            this._roles = result.Data.Roles.Select(x => x.Name).ToHashSet();
        }

        this._users = result.Data.Users.Edges.Select(x => new CursorItem<UserAccountItem>()
        {
            Data = new UserAccountItem
            {
                UserId = x.Node.UserId,
                Username = x.Node.Username,
                Email = x.Node.Email,
                Roles = x.Node.UserRoles.ToList()
            },
            Cursor = x.Cursor
        }).ToList();


        this._onUserBannedSubscription = this.GraphQlClient.UserBanned.Watch().Subscribe(res =>
        {
            if (res.Data is not null)
            {
                var localUser = this._users.FirstOrDefault(x => x.Data.UserId == res.Data.UserBanned.UserId);

                if (localUser is not null)
                {
                    localUser.Data.IsBanned = res.Data.UserBanned.IsBanned;
                    localUser.Data.BanReason = res.Data.UserBanned.BanReason;
                }
            }
        });

        this._onUserDeletedSubscription = this.GraphQlClient.UserDeleted.Watch().Subscribe(res =>
        {
            if (res.Data is not null)
            {
                var localUser = this._users.FirstOrDefault(x => x.Data.UserId == res.Data.UserDeleted.UserId);

                if (localUser is null)
                {
                    return;
                }

                this._users.RemoveAt(this._users.IndexOf(localUser));
                this.StateHasChanged();
            }
        });

        // As users get updated, we'll update the local list of users
        this._onUserUpdatedSubscription = this.GraphQlClient.UserUpdated.Watch().Subscribe(res =>
        {
            if (res.Data is not null)
            {
                var updatedUser = res.Data.UserUpdated;
                var localUser = this._users.FirstOrDefault(x => x.Data.UserId == updatedUser.UserId);

                if (localUser is not null)
                {
                    localUser.Data.Roles = updatedUser.Roles.ToList();
                }

                this.StateHasChanged();
            }
        });
    }

    private async Task UpdateRoleAsync(CursorItem<UserAccountItem> user, bool hasRole, string role)
    {
        if (hasRole)
        {
            var response = await this.GraphQlClient.RemoveRoleFromUser.ExecuteAsync(user.Data.UserId, this.UserId, role);

            if (response.Errors.Any())
            {
                await Console.Error.WriteLineAsync(string.Join("\n", response.Errors.Select(x => x.Message)));
                this.Snack.Add($"An error occurred while removing role from {user.Data.Username}", Severity.Error);
                return;
            }

            if (response.Data?.RemoveRoleFromUser?.Success is true)
            {
                user.Data.Roles.Remove(role);
            }
            else
            {
                this.Snack.Add(response.Data?.RemoveRoleFromUser?.Message ?? "An error occurred", Severity.Error);
            }
        }
        else
        {
            var response = await this.GraphQlClient.AddRoleToUser.ExecuteAsync(user.Data.UserId, this.UserId, role);

            if (response.Errors.Any())
            {
                await Console.Error.WriteLineAsync(string.Join("\n", response.Errors.Select(x => x.Message)));
                this.Snack.Add($"An error occurred while adding role to {user.Data.Username}", Severity.Error);
                return;
            }

            if (response.Data?.AddRoleToUser?.Success is true)
            {
                user.Data.Roles.Add(role);
            }
            else
            {
                this.Snack.Add(response.Data?.AddRoleToUser?.Message ?? "An error occurred", Severity.Error);
            }
        }

        await this.InvokeAsync(this.StateHasChanged);
    }

    public void Dispose()
    {
        this._onUserUpdatedSubscription?.Dispose();
        this._onUserDeletedSubscription?.Dispose();
        this._onUserBannedSubscription?.Dispose();
    }
}