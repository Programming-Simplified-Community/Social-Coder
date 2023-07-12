using Challenges.Common;

namespace Challenges.Core.Services;

public class GuidService : IGuidService
{
    public string Id { get; }

    public GuidService()
    {
        Id = Guid.NewGuid().ToString();
    }
}