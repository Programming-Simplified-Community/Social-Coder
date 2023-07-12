using Challenges.Common;

namespace Challenges.Core.Services;

public class StorageService : IStorageService
{
    private readonly IGuidService _guidService;
    
    public StorageService(IGuidService guidService)
    {
        _guidService = guidService;
        ProjectStoragePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "temp", _guidService.Id);
        ReportsPath = Path.Join(ProjectStoragePath, "Reports");
    }

    public string ProjectStoragePath { get; }
    public string ReportsPath { get; }
    
    public bool ContainsFile(string fileName, out string? filePath)
    {
        var results = Directory.GetFiles(ProjectStoragePath, fileName, SearchOption.AllDirectories);

        if (results.Any())
        {
            filePath = results.First();
            return true;
        }

        filePath = null;
        return false;
    }

    public string[] GetFilesInProject(string? filter = null)
        => Directory.GetFiles(ProjectStoragePath, filter ?? "*", SearchOption.AllDirectories);
}