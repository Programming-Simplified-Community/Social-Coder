namespace Challenges.Core.DockerBuilds;

public record DockerFileResult(bool Success, string Contents, string[]? Errors=null);