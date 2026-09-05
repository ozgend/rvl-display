using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;
using Rvl.Display.Core.Services;

namespace Rvl.Display.Service.Host;

public sealed class RvlDisplayService(
    ILogger<RvlDisplayService> logger,
    IRvlDevice device,
    IRvlSensorMonitor monitor,
    IRvlHidReportSink sink,
    IRvlPipeServer pipeServer,
    RvlTelemetryStreamServer telemetryStream) : BackgroundService
{
    private readonly ILogger<RvlDisplayService> _logger = logger;
    private readonly IRvlDevice _device = device;
    private readonly IRvlSensorMonitor _monitor = monitor;
    private readonly IRvlHidReportSink _sink = sink;
    private readonly IRvlPipeServer _pipeServer = pipeServer;
    private readonly RvlTelemetryStreamServer _telemetryStream = telemetryStream;

    public override async Task StartAsync(CancellationToken ct)
    {
        await base.StartAsync(ct);

        _device.Initialize(async eventName =>
        {
            switch (eventName)
            {
                case Constants.Events.DeviceConnected:
                    _logger?.LogInformation("Device connected: {DeviceInfo}", _device.GetDeviceInfo());
                    await _sink.EnqueueAsync(RvlCommandData.New(Constants.Report.Command.MessageClear), ct);
                    break;
                case Constants.Events.DeviceDisconnected:
                    _logger?.LogInformation("Device disconnected.");
                    break;
                default:
                    _logger?.LogWarning("Unknown device event: {EventName}", eventName);
                    break;
            }
        });

        var canMonitor = _monitor.Initialize();
        if (!canMonitor)
        {
            _logger?.LogError("Failed to initialize LibreHardwareMonitor.");
            await StopAsync(ct);
            return;
        }

        await Task.WhenAll(
            Task.Run(() => _sink.EnqueueAsync(RvlMonitorData.Empty(), ct)),
            Task.Run(() => _sink.EnqueueAsync(RvlCommandData.New(Constants.Report.Command.SetBrightnessHigh), ct)),
            Task.Run(() => _sink.EnqueueAsync(RvlCommandData.New(Constants.Report.Command.MessageClear), ct))
        );

        _logger?.LogInformation("RvlDisplay service started.");
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        // send final signals to device directly, _sink may be stopped
        await Task.WhenAll(
            Task.Run(() => _device.SendAsync(RvlMonitorData.Empty(), ct), ct),
            Task.Run(() => _device.SendAsync(RvlCommandData.New(Constants.Report.Command.MessageCheckHost), ct), ct),
            Task.Run(() => _device.SendAsync(RvlCommandData.New(Constants.Report.Command.SetBrightnessOff), ct), ct)
        );
        await base.StopAsync(ct);

        _logger?.LogInformation("RvlDisplay service stopped.");
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var sinkTask = _sink.RunAsync(ct);
        var pipeTask = _pipeServer.RunAsync(ct);
        var streamTask = _telemetryStream.RunAsync(ct);
        var telemetryTask = RunTelemetryLoopAsync(ct);

        await Task.WhenAll(sinkTask, pipeTask, streamTask, telemetryTask);
    }

    private async Task RunTelemetryLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!_device.IsConnected)
                {
                    await Task.Delay(Constants.DeviceReconnectIntervalMs, ct);
                    continue;
                }

                var data = _monitor.Poll(ct);
                var report = data.ToReport();
                await _sink.EnqueueAsync(report, ct);
                _telemetryStream.Publish(report);
                await Task.Delay(Constants.SensorPollIntervalMs, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Telemetry loop error.");
                await Task.Delay(Constants.DeviceReconnectIntervalMs, ct);
            }
        }
    }
}