namespace Rvl.Display.Core.Interfaces;

public interface IRvlHost
{
    Task RunAsync(CancellationToken ct);
}