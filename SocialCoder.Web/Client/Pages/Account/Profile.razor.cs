using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SocialCoder.Web.Shared.Models;
using SocialCoder.Web.Shared.Models.Account;
using SocialCoder.Web.Shared.ViewModels;
using StrawberryShake;

namespace SocialCoder.Web.Client.Pages.Account;

public partial class Profile : ComponentBase
{
    [Inject] protected SocialCoderGraphQLClient Graph { get; set; }
    [Inject] protected ISnackbar Snack { get; set; }
    [Inject] protected ILogger<Profile> Logger { get; set; }
    [Inject] protected ILocalStorageService Storage { get; set; }
    private string UserId { get; set; }

    private MyProfileInfo ProfileInfo { get; set; } = new();
    private List<UserExperienceViewModel> UserExperiences { get; set; } = [];
    private List<ExperiencePool> ExperiencePool { get; set; } = [];
    private List<UserGoal> Goals { get; set; } = [];
    private readonly MudTheme Theme = new();

    private ExperiencePool? _selectedExperiencePoolItem;
    private Web.Shared.Enums.ExperienceLevel _selectedExperienceLevel = Web.Shared.Enums.ExperienceLevel.White;

    async Task SaveProfileInfo()
    {
        var response = await this.Graph.EditProfileInfo.ExecuteAsync(this.UserId, this.ProfileInfo.Country, this.ProfileInfo.Language, this.ProfileInfo.DisplayName);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            this.Logger.LogError("An error occurred with GraphQL white editing user profile: {Error}", errors);
            this.Snack.Add("An error occurred", Severity.Error);
            return;
        }

        if (!response.Data.EditProfileInfo.Success)
        {
            this.Snack.Add(response.Data.EditProfileInfo.Message, Severity.Error);
            return;
        }

        this.Snack.Add("Profile info updated", Severity.Success);
    }

    async Task AddExperiencePool()
    {
        if (this._selectedExperiencePoolItem is null)
        {
            this.Snack.Add("Please select an experience item.", Severity.Warning);
            return;
        }

        ExperienceLevel level;
        switch (this._selectedExperienceLevel)
        {
            case Web.Shared.Enums.ExperienceLevel.Black:
                level = ExperienceLevel.Black;
                break;
            case Web.Shared.Enums.ExperienceLevel.Blue:
                level = ExperienceLevel.Blue;
                break;
            case Web.Shared.Enums.ExperienceLevel.Green:
                level = ExperienceLevel.Green;
                break;
            case Web.Shared.Enums.ExperienceLevel.Red:
                level = ExperienceLevel.Red;
                break;
            case Web.Shared.Enums.ExperienceLevel.Yellow:
                level = ExperienceLevel.Yellow;
                break;
            default:
                level = ExperienceLevel.White;
                break;
        }

        var response = await this.Graph.AddUserExperience
            .ExecuteAsync(this._selectedExperiencePoolItem.Id, level, this.UserId);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            this.Logger.LogError("An error occurred with GraphQL while adding user experience: {Error}", errors);
            this.Snack.Add("An error occurred", Severity.Error);
            return;
        }

        if (!response.Data.AddUserExperience.Success)
        {
            this.Snack.Add(response.Data.AddUserExperience.Message, Severity.Error);
            return;
        }

        this.UserExperiences.Add(new()
        {
            Experience = this._selectedExperienceLevel,
            Name = this._selectedExperiencePoolItem.Name,
            ImageUrl = this._selectedExperiencePoolItem.ImageUrl,
            ExperiencePoolId = this._selectedExperiencePoolItem.Id,
            UserId = this.UserId
        });

        // Reset values
        this._selectedExperienceLevel = Web.Shared.Enums.ExperienceLevel.White;
        this._selectedExperiencePoolItem = null;

        this.StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        this.UserId = await this.Storage.GetItemAsStringAsync(Constants.UserId);

        var response = await this.Graph.GetProfilePageInfo.ExecuteAsync(this.UserId);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            this.Logger.LogError("GraphQL is misbehaving, {Error}", errors);
            this.Snack.Add("GraphQL Error", Severity.Error);
            return;
        }

        this.ProfileInfo = new()
        {
            Country = response.Data.MyInfo!.Country,
            Username = response.Data.MyInfo.Username,
            DisplayName = response.Data.MyInfo.DisplayName,
            Language = response.Data.MyInfo.Language,
            Email = response.Data.MyInfo.Email
        };

        this.UserExperiences.AddRange(response.Data.UserExperience.Select(x => new UserExperienceViewModel
        {
            Name = x.Name,
            Experience = x.Experience.Translate(),
            ImageUrl = x.ImageUrl,
            ExperiencePoolId = x.ExperiencePoolId
        }));

        this.ExperiencePool.AddRange(response.Data.ExperiencePool.Select(x => new ExperiencePool()
        {
            Name = x.Name,
            ImageUrl = x.ImageUrl,
            Id = x.Id
        }));

        this.Goals.AddRange(response.Data.Goals.Select(x => new UserGoal
        {
            Id = x.Id,
            Description = x.Description,
            Title = x.Title,
            CompletedOn = x.CompletedOn?.DateTime,
            TargetDate = x.TargetDate.DateTime,
            GoalType = (Web.Shared.Enums.GoalType)x.GoalType
        }));
    }
}