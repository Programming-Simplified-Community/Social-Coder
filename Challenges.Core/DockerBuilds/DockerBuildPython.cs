using Challenges.Data.Models;

namespace Challenges.Core.DockerBuilds;

public class DockerBuildPython : DockerBuild
{
    public string MainFile { get; private set; }

    public DockerBuildPython(string languageVersion, string[] files, ScenarioSetup setup) : base(languageVersion, files,
        setup)
    {
        RazorTemplateName = "PythonTemplate.cshtml";
        DetermineMainFile();
    }

    private void DetermineMainFile()
    {
        if (HasFile("main.py", out var mainFile))
        {
            MainFile = new FileInfo(mainFile).Name;
            return;
        }

        if (!HasFile("app.py", out var appFile))
        {
            Errors.Add("Was unable to locate \"main.py\" or \"app.py\" within project context.");
            return;
        }

        MainFile = new FileInfo(appFile).Name;
    }
}