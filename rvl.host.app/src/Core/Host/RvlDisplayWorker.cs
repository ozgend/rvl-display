using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rvl.Host.App.Core.Hardware;
using Rvl.Host.App.Core.Interfaces;
using Rvl.Host.App.Core.Model;

namespace Rvl.Host.App.Core.Host;

public sealed class RvlDisplayWorker(
    ILogger<RvlDisplayWorker> logger,
    IRvlDevice device,
    IRvlSensorMonitor monitor,
    IHidReportSink sink,
    IPipeForwardingServer pipeServer) : BackgroundService
{
    private readonly ILogger<RvlDisplayWorker> _logger = logger;
    private readonly IRvlDevice _device = device;
    private readonly IRvlSensorMonitor _monitor = monitor;
    private readonly IHidReportSink _sink = sink;
    private readonly IPipeForwardingServer _pipeServer = pipeServer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("RvlDisplay service starting...");

        _device.Initialize(async eventName =>
        {
            switch (eventName)
            {
                case Constants.Events.DeviceConnected:
                    _logger?.LogInformation("Device connected: {DeviceInfo}", _device.GetDeviceInfo());
                    await _sink.EnqueueAsync(new RvlCommandData(Constants.Report.Command.MessageClear), stoppingToken);
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
            _logger?.LogError("Failed to initialize LibreHardwareMonitor. Telemetry will be unavailable.");
        }

        var sinkTask = _sink.RunAsync(stoppingToken);
        var pipeTask = _pipeServer.RunAsync(stoppingToken);
        var telemetryTask = RunTelemetryLoopAsync(canMonitor, stoppingToken);

        await Task.WhenAll(sinkTask, pipeTask, telemetryTask);

        _logger?.LogInformation("RvlDisplay service stopped.");
    }

    private async Task RunTelemetryLoopAsync(bool canMonitor, CancellationToken stoppingToken)
    {
        if (!canMonitor)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_device.IsConnected)
                {
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                var data = await _monitor.Poll(stoppingToken);
                await _sink.EnqueueAsync(data, stoppingToken);

                await Task.Delay(Constants.SensorPollIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Telemetry loop error.");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}