using Rvl.Host.App.Core.Model;

namespace Rvl.Host.App.Core.Interfaces;

public interface IRvlSensorMonitor
{
    bool Initialize();
    Task<RvlMonitorData> Poll(CancellationToken ct = default);
}

