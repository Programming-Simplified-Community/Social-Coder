using System.Management.Automation;
using Challenges.Common;

namespace Challenges.Core.Services;

public class ExecutorService : IExecutor
{
    public async Task Run(string command, Action<string?>? infoLog = null, Action<string?>? errorLog = null)
    {
        using var ps = PowerShell.Create();
        ps.AddScript(command);

        if (infoLog is not null)
            ps.Streams.Progress.DataAdding += (sender, args) => infoLog(args.ItemAdded?.ToString());

        if (errorLog is not null)
            ps.Streams.Error.DataAdding += (sender, args) => errorLog(args.ItemAdded?.ToString());

        await ps.InvokeAsync();
    }
}