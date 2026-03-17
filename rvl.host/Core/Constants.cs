namespace Rvl.Display.Core;

public struct Constants
{
    public const string ServiceName = "Rvl.Display.Service";
    public const string CliName = "Rvl.Display.Cli";
    public const string PipeName = "rvl-display";
    public readonly static string PipeAddress = $"\\\\.\\pipe\\{PipeName}";
    public const int SensorPollIntervalMs = 1000;
    public const int DeviceReconnectIntervalMs = 1000;
    public static int PipeConnectTimeoutMs = 1000;

    public struct Events
    {
        public const string DeviceConnected = "DeviceConnected";
        public const string DeviceInterrupted = "DeviceInterrupted";
        public const string DeviceDisconnected = "DeviceDisconnected";
        public const string DeviceError = "DeviceError";
        public const string DeviceNotFound = "DeviceNotFound";
        public const string Unknown = "Unknown";
    }

    public struct DeviceInfo
    {
        public const int VendorId = 0x5EED;
        public const int ProductId = 0xFACE;
        public const string VendorName = "denolk";
        public const string ProductName = "rvl-display";
        public const string SerialNumber = "R0666";
    }

    public struct Report
    {
        public const int Length = 64; // 1 byte for Report ID + 64 bytes for data
        public const byte Null = 0x00;
        public const byte Ok = 0xaa;
        public const byte Error = 0xee;

        public struct PayloadType
        {
            public const byte ReportIdIndex = 0;
            public const byte TypeIndex = 1;
            public const byte Data = 0xdd;
            public const byte Command = 0xcc;
        }

        public struct CommandPayloadIndex
        {
            public const byte CommandName = 1;
            public const byte CommandValue = 2;
        }

        public struct ValuePayloadIndex
        {
            public const byte CpuTemp = 2;
            public const byte CpuUtilization = 3;
            public const byte CpuFan = 4; // +1 for int16_t
            public const byte GpuTemp = 6;
            public const byte GpuUtilization = 7;
            public const byte GpuFan = 8;    // +1 for int16_t
            public const byte ChasisFan = 10; // +1 for int16_t
        }

        public struct Command
        {
            public const byte MessageClear = 0x10;
            public const byte MessageAwait = 0x1a;
            public const byte MessageCheckHost = 0x1c;
            public const byte MessageHighTemp = 0x17;
            public const byte MessageLowRpm = 0x19;

            public const byte ClearDisplay = 0xd0;
            public const byte RestartDevice = 0xff;
            public const byte SetBrightnessOff = 0xb0;
            public const byte SetBrightnessLow = 0xb1;
            public const byte SetBrightnessMedium = 0xb2;
            public const byte SetBrightnessHigh = 0xb3;

            public const byte LedOff = 0xc0;
            public const byte LedOn = 0xc1;

            public const byte EnterBootloader = 0x77;
        }
    }
}