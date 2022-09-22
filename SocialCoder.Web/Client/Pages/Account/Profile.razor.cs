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

    private MyProfileInfo ProfileInfo { get; set; }
    private List<UserExperienceViewModel> UserExperiences { get; set; } = new();
    private List<ExperiencePool> ExperiencePool { get; set; } = new();
    private List<UserGoal> Goals { get; set; } = new();
    private readonly MudTheme Theme = new();

    private ExperiencePool? _selectedExperiencePoolItem;
    private Web.Shared.Enums.ExperienceLevel _selectedExperienceLevel = Web.Shared.Enums.ExperienceLevel.White;

    async Task AddExperiencePool()
    {
        if (_selectedExperiencePoolItem is null)
        {
            Snack.Add("Please select an experience item.", Severity.Warning);
            return;
        }
        //
        // var level = _selectedExperienceLevel switch
        // {
        //     case Web.Shared.Enums.ExperienceLevel.White => ExperienceLevel.White,
        //         _ => ExperienceLevel.White
        // };
        
        var response = await Graph.AddUserExperience
            .ExecuteAsync(_selectedExperiencePoolItem.Id, (ExperienceLevel)(int)_selectedExperienceLevel, UserId);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            Logger.LogError("An error occurred with GraphQL while adding user experience: {Error}", errors);
            Snack.Add("An error occurred", Severity.Error);
            return;
        }

        if (!response.Data.AddUserExperience.Success)
        {
            Snack.Add(response.Data.AddUserExperience.Message, Severity.Error);
            return;
        }
        
        UserExperiences.Add(new()
        {
            Experience = _selectedExperienceLevel,
            Name = _selectedExperiencePoolItem.Name,
            ImageUrl = _selectedExperiencePoolItem.ImageUrl,
            ExperiencePoolId = _selectedExperiencePoolItem.Id,
            UserId = UserId
        });

        // Reset values
        _selectedExperienceLevel = Web.Shared.Enums.ExperienceLevel.White;
        _selectedExperiencePoolItem = null;
        
        StateHasChanged();
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        UserId = await Storage.GetItemAsStringAsync(Constants.UserId);
        
        var response = await Graph.GetProfilePageInfo.ExecuteAsync(UserId);
        
        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            Logger.LogError("GraphQL is misbehaving, {Error}", errors);
            Snack.Add("GraphQL Error", Severity.Error);
            return;
        }

        ProfileInfo = new()
        {
            Country = response.Data.MyInfo!.Country,
            Username = response.Data.MyInfo.Username,
            DisplayName = response.Data.MyInfo.DisplayName,
            Language = response.Data.MyInfo.Language
        };
        
        UserExperiences.AddRange(response.Data.UserExperience.Select(x=>new UserExperienceViewModel
        {
            Name = x.Name,
            Experience = (Web.Shared.Enums.ExperienceLevel)x.Experience,
            ImageUrl = x.ImageUrl,
            ExperiencePoolId = x.ExperiencePoolId
        }));
        
        ExperiencePool.AddRange(response.Data.ExperiencePool.Select(x=>new ExperiencePool()
        {
            Name = x.Name,
            ImageUrl = x.ImageUrl,
            Id = x.Id
        }));
        
        Goals.AddRange(response.Data.Goals.Select(x=>new UserGoal
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