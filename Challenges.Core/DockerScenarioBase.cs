using Challenges.Common;
using Challenges.Core.DockerBuilds;
using Challenges.Core.Exceptions;
using Challenges.Core.Parsers;
using Challenges.Data.Models;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Challenges.Core;

public abstract class DockerScenarioBase : IDisposable, IAsyncDisposable
{
    protected readonly ILogger<DockerScenarioBase> Logger;
    protected readonly DockerClient DockerClient = new DockerClientConfiguration().CreateClient();
    protected CreateContainerResponse CreateResponse = null!;
    protected readonly IConfiguration Configuration;
    protected IGuidService GuidService;
    protected IStorageService StorageService;
    protected IExecutor Executor;

    protected CancellationTokenSource PrimaryTokenSource = new();
    protected CancellationToken ScenarioToken { get; private set; }
    protected Scenario CurrentScenario { get; private set; }
    protected ScenarioSetup CurrentSetup { get; private set; }
    protected ScenarioAttempt CurrentAttempt { get; private set; }
    private DateTime? _containerStartTime;
    public TimeSpan? ContainerDuration { get; private set; }
    private string _dockerFilePath;

    public DockerScenarioBase(
        IConfiguration configuration,
        IGuidService guidService,
        IExecutor executor,
        ILogger<DockerScenarioBase> logger,
        IStorageService storageService)
    {
        Configuration = configuration;
        GuidService = guidService;
        StorageService = storageService;
        Executor = executor;
        Logger = logger;
    }

    public void SetScenario(Scenario scenario, ScenarioSetup setup, ScenarioAttempt attempt)
    {
        CurrentSetup = setup;
        CurrentAttempt = attempt;
        CurrentScenario = scenario;
    }

    private async Task<ScenarioReport?> ParseReport()
    {
        IParser? parser = CurrentAttempt.Language switch
        {
            ProgrammingLanguage.Python => new PyTestParser(),
            _ => throw new NotImplementedException($"{CurrentAttempt.Language} is not implemented")
        };

        var files = Directory.GetFiles(StorageService.ReportsPath);

        if (!files.Any())
        {
            Logger.LogWarning("Was unable to locate a report at {ReportsPath}. Unable to generate a report...",
                StorageService.ReportsPath);
            return null;
        }

        var jsonText = await File.ReadAllTextAsync(files.First(), ScenarioToken);

        return await parser.ParseJson(jsonText);
    }

    public async Task<ScenarioReport?> Run(CancellationToken token = default)
    {
        ScenarioToken = token;
        ScenarioReport? report = null;

        try
        {
            await InitRepository();
            await CreateDockerFile();
            await CreateContainer();
            await StartContainer();
            report = await ParseReport();
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to run. {Exception}", ex);
            PrimaryTokenSource?.Cancel();
        }
        finally
        {
            await DisposeAsync();
        }

        return report;
    }

    /// <summary>
    /// Clone project into /temp/scopedId/RepoGoesHere
    /// </summary>
    protected virtual async Task InitRepository()
    {
        await Executor.Run($"cd \"{StorageService.ProjectStoragePath}\" && git clone {CurrentAttempt.GithubUrl} .");
    }

    private async Task CreateDockerFile()
    {
        DockerBuild builder = CurrentSetup.Language switch
        {
            ProgrammingLanguage.Python => new DockerBuildPython(CurrentSetup.LanguageVersion,
                StorageService.GetFilesInProject(), CurrentSetup),
            _ => throw new NotSupportedException(
                $"{CurrentSetup.Language} - has not been implemented, or is not supported")
        };

        var generationResult = await builder.GenerateDockerFile();
        var dockerFilePath = Path.Join(StorageService.ProjectStoragePath, $"Dockerfile-{GuidService.Id}");

        if (!generationResult.Success)
        {
            var errors = string.Join("\t\n", generationResult?.Errors ?? Array.Empty<string>());
            Logger.LogError(
                "An error occurred while generating Dockerfile for \nId: {Id}\nPath: {Path}\nErrors: {Errors}",
                GuidService.Id, dockerFilePath, errors);
            throw new DockerFileGenerationException(errors);
        }

        Logger.LogInformation("Creating Dockerfile: {Id}\nPath: {Path}\n\n{Contents}", GuidService.Id,
            dockerFilePath,
            generationResult);

        await File.WriteAllTextAsync(dockerFilePath, generationResult.Contents, ScenarioToken);
        _dockerFilePath = dockerFilePath;
    }

    private async Task CreateContainer()
    {
        // ensure we have the most up to date version of the tagged image
        await Executor.Run($"docker pull {CurrentSetup.DockerImage}",
            (x)=>Logger.LogInformation(x),
                (x)=>Logger.LogError(x));

        List<string> mounts = new()
        {
            $"{StorageService.ReportsPath}:/app/Data/Reports:rw"
        };

        Logger.LogWarning("Mounting Dirs:\b\t{Dirs}", string.Join("\n\t", mounts));

        CreateResponse = await DockerClient.Containers.CreateContainerAsync(new()
        {
            Image = GuidService.Id,
            Entrypoint = CurrentSetup.DockerEntrypoint.Split(' '),
            AttachStderr = true,
            AttachStdout = true,
            HostConfig = new()
            {
                Binds = mounts.ToArray()
            },
            NetworkDisabled = true
        }, ScenarioToken);
    }

    private async Task StartContainer()
    {
        var startTime = DateTime.UtcNow;
        var success = await DockerClient.Containers.StartContainerAsync(CreateResponse.ID, new());

        if (!success)
        {
            Logger.LogError(
                "Was unable to start container for:\nProject Id: {Id}\nImage: {Image}\nScenario Id: {Scenario}",
                GuidService.Id, CurrentSetup.DockerImage, CurrentSetup.Id);
            throw new OperationCanceledException("Was unable to start project container");
        }

        CancellationTokenSource cts = new(); // this token is for stopping container
        await AttachToContainer(cts);

        var endTime = DateTime.UtcNow;
        ContainerDuration = endTime - startTime;

        Logger.LogWarning("Project Id: {Id} took {Time}",
            GuidService.Id,
            $"{(ContainerDuration):g}");

        await DockerClient.Containers.RemoveContainerAsync(CreateResponse.ID, new()
        {
            Force = true
        }, ScenarioToken);

        await Executor.Run($"docker rmi -f {GuidService.Id}");
    }

    private async Task AttachToContainer(CancellationTokenSource cts)
    {
        _containerStartTime = DateTime.UtcNow;

        try
        {
            await DockerClient.Containers.GetContainerStatsAsync(CreateResponse.ID, new()
            {
                Stream = true
            }, ContainerProgress(cts), cts.Token);
        }
        catch
        {
            // Ignore
        }
    }

    private Progress<ContainerStatsResponse> ContainerProgress(CancellationTokenSource cts)
        => new(response =>
        {
            if (response.CPUStats.CPUUsage.TotalUsage <= 0)
                cts.Cancel();

            // if the user's code elapses our timeout we'll shut things down
            if (CurrentScenario.TimeoutInMinutes.HasValue &&
                (DateTime.UtcNow - _containerStartTime.Value).TotalMinutes >= CurrentScenario.TimeoutInMinutes)
                cts.Cancel();

            ProcessContainerStats(response);
        });

    protected virtual void ProcessContainerStats(ContainerStatsResponse stats)
    {
        
    }

    public virtual void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        Logger.LogInformation("Cleaning up Docker Container Scenario...");

        Logger.LogInformation("Recursively deleting project path: {Path}", StorageService.ProjectStoragePath);
        if (Directory.Exists(StorageService.ProjectStoragePath))
            Directory.Delete(StorageService.ProjectStoragePath, true);

        Logger.LogInformation("Cleaning up container: {Id}", GuidService.Id);
        await DockerClient.Containers.RemoveContainerAsync(GuidService.Id, new()
        {
            Force = true
        }, ScenarioToken);

        Logger.LogInformation("Cleaning up scenario image: {Id}", GuidService.Id);
        await DockerClient.Images.DeleteImageAsync(CreateResponse.ID, new()
        {
            Force = true,
            NoPrune = false
        }, ScenarioToken);
    }
}