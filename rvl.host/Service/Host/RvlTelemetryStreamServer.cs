using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Rvl.Display.Core;

namespace Rvl.Display.Service.Host;

public sealed class RvlTelemetryStreamServer(ILogger<RvlTelemetryStreamServer> logger)
{
    private readonly ILogger<RvlTelemetryStreamServer> _logger = logger;
    private readonly Channel<byte[]> _reports = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(1)
    {
        SingleReader = true,
        SingleWriter = true,
        FullMode = BoundedChannelFullMode.DropOldest
    });

    public void Publish(byte[] report)
    {
        _reports.Writer.TryWrite(report);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await using var pipe = CreateServerStream();
            try
            {
                await pipe.WaitForConnectionAsync(ct);
                _logger.LogInformation("Telemetry stream client connected.");

                while (pipe.IsConnected && !ct.IsCancellationRequested)
                {
                    var report = await _reports.Reader.ReadAsync(ct);
                    await pipe.WriteAsync(report, ct);
                    await pipe.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                _logger.LogInformation("Telemetry stream client disconnected.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telemetry stream error.");
            }
        }
    }

    private static NamedPipeServerStream CreateServerStream()
    {
        var pipeSecurity = new PipeSecurity();
        var localSystemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        var authenticatedUsersSid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

        pipeSecurity.AddAccessRule(new PipeAccessRule(authenticatedUsersSid, PipeAccessRights.Read, AccessControlType.Allow));
        pipeSecurity.AddAccessRule(new PipeAccessRule(localSystemSid, PipeAccessRights.FullControl, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            Constants.StreamPipeName,
            PipeDirection.Out,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            0,
            Constants.Report.Length,
            pipeSecurity);
    }
}