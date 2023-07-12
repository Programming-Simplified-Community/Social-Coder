using Challenges.Data.Models;

namespace Challenges.Core.DockerBuilds;

public class DockerBuildCSharp : DockerBuild
{
    public string ProjectDLLName { get; private set; }
    public string CSProjectDirectoryName { get; private set; }
    public string CSProjectFile { get; private set; }

    public string CSProjectFilePath => $"{CSProjectDirectoryName}/{CSProjectFile}";

    public DockerBuildCSharp(string languageVersion, string[] files, ScenarioSetup setup) : base(languageVersion, files, setup)
    {
        RazorTemplateName = "CSharpTemplate.cshtml";
        DetermineOutputDLL();
    }

    private void DetermineOutputDLL()
    {
        if (!HasFiles(".csproj", out var csProjectFiles))
        {
            Errors.Add("Unable to locate \".csproj\" files.");
            return;
        }

        if (!HasFile("Program.cs", out var programFile))
        {
            Errors.Add("Unable to locate \"Program.cs\"");
            return;
        }

        var programInfo = new FileInfo(programFile);
        var projectFile = csProjectFiles.FirstOrDefault(x => x.StartsWith(programInfo.DirectoryName));

        if (projectFile is null)
        {
            Errors.Add($"Could not locate \".csproj\" file adjacent to {programFile}");
            return;
        }

        var projectDirectory = new DirectoryInfo(projectFile);
        var fileInfo = new FileInfo(projectFile);
        CSProjectDirectoryName = projectDirectory.Name;
        CSProjectFile = fileInfo.Name;
        ProjectDLLName = $"{fileInfo.Name.Split('.').FirstOrDefault()}.dll";
    }
}