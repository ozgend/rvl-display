using System.ServiceProcess;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;

namespace Rvl.Display.Service.Host;

public sealed class RvlWindowsServiceLifetime : WindowsServiceLifetime
{
    private readonly IRvlHidReportSink _sink;
    private readonly ILogger<RvlWindowsServiceLifetime> _logger;

    public RvlWindowsServiceLifetime(
        IHostEnvironment environment,
        IHostApplicationLifetime hostApplicationLifetime,
        ILoggerFactory loggerFactory,
        IOptions<HostOptions> optionsAccessor,
        IRvlHidReportSink sink)
        : base(environment, hostApplicationLifetime, loggerFactory, optionsAccessor)
    {
        _sink = sink;
        _logger = loggerFactory.CreateLogger<RvlWindowsServiceLifetime>();

        // Enable power status change notifications
        CanHandlePowerEvent = true;
    }

    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
    {
        _logger.LogInformation("Windows Power Event received: {PowerStatus}", powerStatus);

        if (powerStatus == PowerBroadcastStatus.Suspend)
        {
            _logger.LogInformation("System suspending. Sending Check Host...");

            // Sending directly as a fire-and-forget before OS suspends
            _ = Task.Run(async () =>
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await _sink.EnqueueAsync(RvlMonitorData.Empty(), cts.Token);
                    await _sink.EnqueueAsync(RvlCommandData.New(Constants.Report.Command.MessageCheckHost), cts.Token);
                    await _sink.EnqueueAsync(RvlCommandData.New(Constants.Report.Command.SetBrightnessOff), cts.Token);
                    _logger.LogInformation("Check Host signal sent for suspension.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send Check Host during service suspend event.");
                }
            });
        }

        if (powerStatus == PowerBroadcastStatus.ResumeSuspend || powerStatus == PowerBroadcastStatus.ResumeAutomatic || powerStatus == PowerBroadcastStatus.ResumeCritical)
        {
            _logger.LogInformation("System resuming. Sending Check Host...");

            // Sending directly as a fire-and-forget after OS resumes
            _ = Task.Run(async () =>
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await _sink.EnqueueAsync(RvlMonitorData.Empty(), cts.Token);
                    await _sink.EnqueueAsync(RvlCommandData.New(Constants.Report.Command.SetBrightnessHigh), cts.Token);
                    await _sink.EnqueueAsync(RvlCommandData.New(Constants.Report.Command.MessageClear), cts.Token);
                    _logger.LogInformation("Check Host signal sent for resumption.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send Check Host during service resume event.");
                }
            });
        }

        return base.OnPowerEvent(powerStatus);
    }
}
