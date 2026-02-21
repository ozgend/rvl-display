using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Rvl.Host.App.Core.Hardware;
using Rvl.Host.App.Core.Interfaces;

namespace Rvl.Host.App.Core.Host;

public sealed class HidReportSink : IHidReportSink
{
    private readonly ILogger<HidReportSink> _logger;
    private readonly IRvlDevice _device;

    private readonly Channel<byte[]> _channel;

    public HidReportSink(ILogger<HidReportSink> logger, IRvlDevice device)
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
        _logger?.LogInformation("HID sink loop started.");

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

        _logger?.LogInformation("HID sink loop stopped.");
    }
}