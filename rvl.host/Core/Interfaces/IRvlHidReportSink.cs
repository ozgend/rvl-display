namespace Rvl.Display.Core.Interfaces;

public interface IRvlHidReportSink : IRvlHost
{
    ValueTask EnqueueAsync(byte[] report64, CancellationToken ct);
    ValueTask EnqueueAsync<TStruct>(IRvlDevicePayload<TStruct> payload, CancellationToken ct) where TStruct : struct;
}

