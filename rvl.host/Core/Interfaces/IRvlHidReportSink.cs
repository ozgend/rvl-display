namespace Rvl.Display.Core.Interfaces;

public interface IRvlHidReportSink : IRvlHost
{
    ValueTask EnqueueAsync(byte[] report64, CancellationToken ct);
    ValueTask EnqueueAsync<T>(IRvlDevicePayload<T> payload, CancellationToken ct);
}

