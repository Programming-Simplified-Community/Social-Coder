namespace Challenges.Data.Models;

public class ScenarioTestResult
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to <see cref="ScenarioReport"/>
    /// </summary>
    public int ScenarioReportId { get; set; }
    
    /// <summary>
    /// Name of test
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Time it took to execute test
    /// </summary>
    public string? Duration { get; set; }
    
    /// <summary>
    /// Total number of runs for this method/test case
    /// </summary>
    public int TotalRuns { get; set; }
    
    /// <summary>
    /// Total number of failed runs for this method/test case
    /// </summary>
    public int TotalFails { get; set; }
    
    /// <summary>
    /// The values that were passed in that lead to a failure
    /// </summary>
    /// <remarks>
    /// This will be delimited by triple pipes |||
    /// </remarks>
    public string? IncomingValues { get; set; }

    /// <summary>
    /// Messages that showcase what went wrong
    /// </summary>
    /// <remarks>
    /// This will be delimited by triple pipes |||
    /// </remarks>
    public string? AssertionValues { get; set; }

    /// <summary>
    /// Pass/Fail
    /// </summary>
    public TestStatus Status { get; set; }
}

public enum TestStatus
{
    Pass,Fail
}