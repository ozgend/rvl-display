namespace Rvl.Host.App.Core.Interfaces;

public interface IHidReportSink
{
    ValueTask EnqueueAsync(byte[] report64, CancellationToken ct);
    ValueTask EnqueueAsync<T>(IRvlDevicePayload<T> payload, CancellationToken ct);
    Task RunAsync(CancellationToken ct);
}

