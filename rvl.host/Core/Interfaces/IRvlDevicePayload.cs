namespace Rvl.Display.Core.Interfaces;

public interface IRvlDevicePayload<T>
{
    byte Type { get; }
    byte[] ToReport();
}

