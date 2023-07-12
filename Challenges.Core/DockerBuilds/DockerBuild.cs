using Challenges.Data.Models;
using Razor.Templating.Core;

namespace Challenges.Core.DockerBuilds;

public class DockerBuild
{
    public string[] ProjectFiles { get; init; }
    public string LanguageVersion { get; init; }
    public List<string> Errors { get; private set; } = new();
    public ScenarioSetup CurrentSetup { get; init; }

    private const string TemplateFolder = "~/Views/Docker";
    
    /// <summary>
    /// Name of docker file template to use
    /// </summary>
    public string RazorTemplateName { get; set; }
    
    /// <summary>
    /// Path to template file
    /// </summary>
    public string TemplatePath => $"{TemplateFolder}/{RazorTemplateName}";

    public DockerBuild(string languageVersion, string[] files, ScenarioSetup setup)
    {
        ProjectFiles = files;
        LanguageVersion = languageVersion;
        CurrentSetup = setup;
    }

    public async Task<DockerFileResult> GenerateDockerFile()
    {
        if (Errors.Any())
            return new(false, string.Empty, Errors.ToArray());

        var result = await RazorTemplateEngine.RenderAsync(TemplatePath, this);

        return new(true, result);
    }

    public virtual string GetDockerfileSymbolicReportLink()
        => $"""
            # MAGIC NEEDED
            RUN mkdir -p /reports
            RUN mkdir -p {CurrentSetup.InsideContainerMountLocation}/Data/Reports
            RUN ln -s {CurrentSetup.InsideContainerMountLocation}/Data/Reports /reports
            """;

    public bool HasFiles(string fileName, out string[]? filePaths)
    {
        filePaths = Array.Empty<string>();

        if (!ProjectFiles.Any())
            return false;

        var paths = ProjectFiles.Where(x => x.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (!paths.Any())
            return false;

        filePaths = paths.ToArray();
        return true;
    }

    public bool HasFile(string fileName, out string? filePath)
    {
        filePath = null;

        if (!ProjectFiles.Any())
            return false;

        var path = ProjectFiles.FirstOrDefault(x => x.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (path is null)
            return false;

        filePath = path;
        return true;
    }
}