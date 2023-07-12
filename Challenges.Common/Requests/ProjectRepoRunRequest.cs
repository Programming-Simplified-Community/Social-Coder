using Challenges.Data.Models;

namespace Challenges.Common.Requests;

public class ProjectRepoRunRequest
{
    public int ScenarioId { get; set; }
    
    public ProgrammingLanguage Language { get; set; }
    
    public string UserId { get; set; }
    
    public string? GithubRepo { get; set; }
    public string? GithubBranch { get; set; }
    public string? EncodedSubmission { get; set; }
}