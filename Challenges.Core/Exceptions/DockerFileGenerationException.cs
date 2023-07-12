namespace Challenges.Core.Exceptions;

public class DockerFileGenerationException : Exception
{
    public DockerFileGenerationException(string message) : base(message)
    {
        
    }
}