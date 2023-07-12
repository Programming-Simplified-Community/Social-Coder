namespace Challenges.Common.Notifications;

public class ProjectRepoRunResponse
{
    public int? QueuePosition { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static ProjectRepoRunResponse Success(int position) => new() { IsSuccess = true, QueuePosition = position };
    public static ProjectRepoRunResponse Fail(string message) => new() { IsSuccess = false, ErrorMessage = message };
}