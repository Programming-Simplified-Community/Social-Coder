using System.Globalization;
using Challenges.Common;
using Challenges.Data.Models;
using Newtonsoft.Json.Linq;

namespace Challenges.Core.Parsers;

public class PyTestParser : IParser
{
    enum Outcome
    {
        Failed,
        Passed
    }

    class PyTestReport
    {
        public decimal Duration { get; set; }
        public PyTestSummary Summary { get; set; }
    }

    class PyTestSummary
    {
        public int Failed { get; set; }
        public int Passed { get; set; }
        public int Total { get; set; }
    }

    class PyTestContainer
    {
        public List<double> Duration { get; } = new();
        public List<string> Assertions { get; } = new();
        public List<string> Incoming { get; } = new();
        public string Name { get; set; }
        public int Total { get; set; }
        public int Failed { get; set; }
        public bool Passed => Total > 0 && Failed == 0;
    }
    
    public async Task<ScenarioReport?> ParseJson(string jsonResult)
    {
        if (string.IsNullOrWhiteSpace(jsonResult))
            return null;

         try
        {
            var root = JObject.Parse(jsonResult);

            PyTestReport report = new();

            int.TryParse(root["summary"]!["passed"]?.ToString(), out var passed);
            int.TryParse(root["summary"]!["failed"]?.ToString(), out var failed);
            int.TryParse(root["summary"]!["total"]?.ToString(), out var total);

            report.Summary = new()
            {
                Failed = failed,
                Passed = passed,
                Total = total
            };

            var testArray = (JArray) root["tests"]!;
            
            // we need a way of tracking multiple test-cases for the same test
            Dictionary<string, PyTestContainer> namedTests = new();

            foreach (var item in testArray)
            {
                var id = item["nodeid"]!.ToObject<string>();

                if (string.IsNullOrWhiteSpace(id)) continue;

                var start = id.IndexOf(':') + 2;
                var end = id.IndexOf('[');

                var testName = end < 0
                    ? id.Substring(start)
                    : id.Substring(start, Math.Max(end - start, 1));

                if (!namedTests.ContainsKey(testName))
                    namedTests.Add(testName, new()
                    {
                        Name = testName
                    });

                var outcome = item["outcome"]?.ToString() switch
                {
                    "passed" => Outcome.Passed,
                    _ => Outcome.Failed
                };

                if (outcome == Outcome.Failed)
                {
                    namedTests[testName].Failed += 1;
                    namedTests[testName].Assertions
                        .Add(item["call"]?["crash"]?["message"]?.ToObject<string>() ?? string.Empty);

                    var longrep = item["call"]?["longrepr"]?.ToObject<string>() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(longrep))
                    {
                        var parameterEnd = longrep.IndexOf("\n\n");
                        longrep = longrep[..parameterEnd];
                    }

                    namedTests[testName].Incoming.Add(longrep);
                }

                namedTests[testName].Total++;
                namedTests[testName].Duration.Add(item["call"]?["duration"]?.ToObject<double>() ?? -1);
            }

            var scenario = new ScenarioReport();
            double totalTime = 0;

            foreach (var container in namedTests.Values)
            {
                var average = container.Duration.Average();
                scenario.ScenarioTestResults.Add(new()
                {
                    Name = container.Name,
                    Status = container.Passed ? TestStatus.Pass : TestStatus.Fail,
                    Duration = average.ToString(CultureInfo.InvariantCulture),
                    AssertionValues = string.Join("|||", container.Assertions),
                    IncomingValues = string.Join("|||", container.Incoming),
                    TotalFails = container.Failed,
                    TotalRuns = container.Total
                });

                totalTime += average;
            }

            report.Duration = (decimal)totalTime;
            return scenario;
        }
        catch
        {
            return null;
        }
    }
}