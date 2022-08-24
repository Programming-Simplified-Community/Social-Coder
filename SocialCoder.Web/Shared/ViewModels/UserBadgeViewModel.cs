using SocialCoder.Web.Shared.Models;

namespace SocialCoder.Web.Shared.ViewModels;

public class UserBadgeViewModel
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public int Progress { get; init; }
    public int Requirement { get; init; }
    public bool IsCompleted => Progress >= Requirement;
    public string ImagePath { get; init; }
    public BadgeType Type { get; init; }
}