using Challenges.Common.Notifications;
using Challenges.Common.Requests;

namespace Challenges.Common;

public interface IProjectRunService
{
    /// <summary>
    /// Queue project for testing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ProjectRepoRunResponse> QueueProject(ProjectRepoRunRequest request);

    /// <summary>
    /// Current number of projects in-queue waiting for testing
    /// </summary>
    /// <returns></returns>
    Task<int> QueueCount();
}