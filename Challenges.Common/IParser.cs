using Challenges.Data.Models;

namespace Challenges.Common;

public interface IParser
{
    Task<ScenarioReport?> ParseJson(string jsonResult);
}