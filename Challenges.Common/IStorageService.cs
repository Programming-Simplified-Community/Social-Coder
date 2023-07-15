﻿namespace Challenges.Common;

public interface IStorageService
{
    /// <summary>
    /// Location of project
    /// </summary>
    string ProjectStoragePath { get; }
    
    /// <summary>
    /// Where do reports generated by the service go?
    /// </summary>
    string ReportsPath { get; }
    
    /// <summary>
    /// Checks to see if the <see cref="ProjectStoragePath"/> contains the specified <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">File to check for</param>
    /// <param name="filePath">If file is found, this variable is set to its path</param>
    /// <returns>True if file name exists, and outputs its location to <paramref name="filePath"/></returns>
    bool ContainsFile(string fileName, out string? filePath);
    
    /// <summary>
    /// Retrieve files from <see cref="ProjectStoragePath"/>. Optionally, can apply a filter on results
    /// </summary>
    /// <param name="filter">Optional filter. Otherwise, returns all files</param>
    /// <returns>Files from project path</returns>
    string[] GetFilesInProject(string? filter = null);
}