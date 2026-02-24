using Rvl.Display.Core.Model;

namespace Rvl.Display.Core.Interfaces;

public interface IRvlSensorMonitor
{
    bool Initialize();
    Task<RvlMonitorData> Poll(CancellationToken ct = default);
}

