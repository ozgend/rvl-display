namespace Rvl.Host.App.Core.Interfaces;

public interface IRvlDevicePayload<T>
{
    byte Type { get; }
    byte[] ToReport();
}

