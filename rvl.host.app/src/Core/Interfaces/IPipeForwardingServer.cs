namespace Rvl.Host.App.Core.Interfaces;

public interface IPipeForwardingServer
{
    string PipeName { get; }
    Task RunAsync(CancellationToken ct);
}

