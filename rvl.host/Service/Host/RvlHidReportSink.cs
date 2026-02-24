using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Service.Hardware;

namespace Rvl.Display.Service.Host;

public sealed class RvlHidReportSink : IRvlHidReportSink
{
    private readonly ILogger<RvlHidReportSink> _logger;
    private readonly IRvlDevice _device;

    private readonly Channel<byte[]> _channel;

    public RvlHidReportSink(ILogger<RvlHidReportSink> logger, IRvlDevice device)
    {
        _logger = logger;
        _device = device;

        _channel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(capacity: 256)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public ValueTask EnqueueAsync(byte[] report64, CancellationToken ct)
    {
        if (report64.Length != Constants.Report.Length)
        {
            _logger?.LogError("Invalid report length: {Length}. Expected: {ExpectedLength}", report64.Length, Constants.Report.Length);
            return new ValueTask();
        }
        return _channel.Writer.WriteAsync(report64, ct);
    }

    public ValueTask EnqueueAsync<T>(IRvlDevicePayload<T> payload, CancellationToken ct)
    {
        var report = payload.ToReport();
        return EnqueueAsync(report, ct);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        _logger?.LogDebug("HID sink loop started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var report = await _channel.Reader.ReadAsync(ct);

                if (!_device.IsConnected)
                {
                    continue;
                }

                await _device.SendRawAsync(report, ct);

            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "HID sink loop error.");
                await Task.Delay(250, ct);
            }
        }

        _logger?.LogDebug("HID sink loop stopped.");
    }
}