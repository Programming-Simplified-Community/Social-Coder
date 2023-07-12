namespace Challenges.Common;

public interface IExecutor
{
    Task Run(string command, Action<string?>? infoLog = null, Action<string?>? errorLog = null);
}