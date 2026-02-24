using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;

namespace Rvl.Display.Service.Host;

public sealed class RvlPipeServer(ILogger<RvlPipeServer> logger, IRvlHidReportSink sink) : IRvlPipeServer
{
    public string PipeName => Constants.PipeName;
    private readonly ILogger<RvlPipeServer> _logger = logger;
    private readonly IRvlHidReportSink _sink = sink;

    public async Task RunAsync(CancellationToken ct)
    {
        _logger?.LogInformation("Named pipe server starting on \\\\.\\pipe\\{PipeName}", PipeName);

        while (!ct.IsCancellationRequested)
        {
            var server = CreateServerStream();
            try
            {
                await server.WaitForConnectionAsync(ct);
                _ = HandleClientAsync(server, ct);
            }
            catch (OperationCanceledException)
            {
                server.Dispose();
                break;
            }
            catch (Exception ex)
            {
                server.Dispose();
                _logger?.LogError(ex, "Pipe accept loop error.");
                await Task.Delay(250, ct);
            }
        }

        _logger?.LogInformation("Named pipe server stopped.");
    }

    private NamedPipeServerStream CreateServerStream()
    {
        var pipeSecurity = new PipeSecurity();
        var localSystemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        var authenticatedUsersSid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

        pipeSecurity.AddAccessRule(new PipeAccessRule(authenticatedUsersSid, PipeAccessRights.ReadWrite, AccessControlType.Allow));
        pipeSecurity.AddAccessRule(new PipeAccessRule(localSystemSid, PipeAccessRights.FullControl, AccessControlType.Allow));

        var pipeServer = NamedPipeServerStreamAcl.Create(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, inBufferSize: 4096, outBufferSize: 4096, pipeSecurity);
        return pipeServer;
    }

    // ipc client handler
    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        _logger?.LogInformation("Pipe client connected.");
        try
        {
            var buffer = new byte[Constants.Report.Length];

            while (pipe.IsConnected && !ct.IsCancellationRequested)
            {
                var read = 0;
                while (read < buffer.Length)
                {
                    var n = await pipe.ReadAsync(buffer.AsMemory(read, buffer.Length - read), ct);
                    if (n == 0)
                    {
                        return;
                    }
                    read += n;
                }

                try
                {
                    var report = new byte[Constants.Report.Length];
                    Buffer.BlockCopy(buffer, 0, report, 0, buffer.Length);

                    await _sink.EnqueueAsync(report, ct);
                    await pipe.WriteAsync(new byte[] { Constants.Report.Ok }, ct);
                    await pipe.FlushAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to enqueue forwarded report.");
                    await pipe.WriteAsync(new byte[] { Constants.Report.Error }, ct);
                    await pipe.FlushAsync(ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Pipe client handler error.");
        }
        finally
        {
            try
            {
                pipe.Dispose();
            }
            catch
            {
                // ignore
            }
            _logger?.LogInformation("Pipe client disconnected.");
        }
    }
}
