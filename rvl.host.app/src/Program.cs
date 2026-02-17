using Rvl.Host.App;

var device = new RvlDevice();
var hasDevice = device.Initialize();

if (!hasDevice)
{
    Console.WriteLine("Warning: Failed to initialize RvlDevice.");
}

var sensorMonitor = new RvlSensorMonitor();
var canMonitor = sensorMonitor.Initialize();

if (!canMonitor)
{
    Console.WriteLine("Error: Failed to initialize monitor.");
    if (hasDevice)
    {
        await device.SendCommand(Constants.Report.Command.MessageCheckHost);
        device.Dispose();
    }
    Environment.Exit(1);
}

using var cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Ctrl+C received. Exiting...");
    e.Cancel = true;
    cancellation.Cancel();
};

Console.Clear();
Console.CursorVisible = false;
Console.WriteLine("Rvl.Host.App | exit=[CTRL+C] | bootloader=[F] | ledON=[O] | ledOFF=[P] | brightness=[B/M/N] | clear=[C] | restart=[R] | message=[CTRL+1..4]");

int startCol = 0;
int startRow = 1;

if (hasDevice)
{
    await device.SendCommand(Constants.Report.Command.LedOff);
}

try
{
    while (!cancellation.Token.IsCancellationRequested)
    {
        if (hasDevice && Console.KeyAvailable)
        {
            switch (Console.ReadKey(true).Key)
            {
                // ctrl+1
                case ConsoleKey.D1 when (ConsoleModifiers.Control & ConsoleModifiers.Control) != 0:
                    await device.SendCommand(Constants.Report.Command.MessageAwait);
                    Console.WriteLine("Sent MessageAwait command to device.");
                    break;
                case ConsoleKey.D2 when (ConsoleModifiers.Control & ConsoleModifiers.Control) != 0:
                    await device.SendCommand(Constants.Report.Command.MessageCheckHost);
                    Console.WriteLine("Sent MessageCheckHost command to device.");
                    break;
                case ConsoleKey.D3 when (ConsoleModifiers.Control & ConsoleModifiers.Control) != 0:
                    await device.SendCommand(Constants.Report.Command.MessageHighTemp);
                    Console.WriteLine("Sent MessageHighTemp command to device.");
                    break;
                case ConsoleKey.D4 when (ConsoleModifiers.Control & ConsoleModifiers.Control) != 0:
                    await device.SendCommand(Constants.Report.Command.MessageLowRpm);
                    Console.WriteLine("Sent MessageLowRpm command to device.");
                    break;

                case ConsoleKey.C:
                    await device.SendCommand(Constants.Report.Command.ClearDisplay);
                    Console.WriteLine("Sent ClearDisplay command to device.");
                    break;
                case ConsoleKey.R:
                    await device.SendCommand(Constants.Report.Command.RestartDevice);
                    Console.WriteLine("Sent RestartDevice command to device.");
                    break;
                case ConsoleKey.B:
                    await device.SendCommand(Constants.Report.Command.SetBrightnessHigh);
                    Console.WriteLine("Sent SetBrightnessHigh command to device.");
                    break;
                case ConsoleKey.N:
                    await device.SendCommand(Constants.Report.Command.SetBrightnessMedium);
                    Console.WriteLine("Sent SetBrightnessMedium command to device.");
                    break;
                case ConsoleKey.M:
                    await device.SendCommand(Constants.Report.Command.SetBrightnessLow);
                    Console.WriteLine("Sent SetBrightnessLow command to device.");
                    break;

                case ConsoleKey.O:
                    await device.SendCommand(Constants.Report.Command.LedOn);
                    Console.WriteLine("Sent LedOn command to device.");
                    break;
                case ConsoleKey.P:
                    await device.SendCommand(Constants.Report.Command.LedOff);
                    Console.WriteLine("Sent LedOff command to device.");
                    break;

                case ConsoleKey.F:
                    await device.SendCommand(Constants.Report.Command.EnterBootloader);
                    Console.WriteLine("Sent EnterBootloader command to device. Exiting...");
                    // cancellation.Cancel();
                    break;
            }
        }

        var data = sensorMonitor.Poll();

        Console.SetCursorPosition(startCol, startRow);
        Console.WriteLine($"CPU: {data.CpuName,-16} - {data.CpuTemperature,4:F1}°C, {data.CpuUtilization,5:F1}%   ");
        Console.WriteLine($"GPU: {data.GpuName,-16} - {data.GpuTemperature,4:F1}°C, {data.GpuUtilization,5:F1}%   ");

        if (hasDevice)
        {
            await device.SendData(data);
            var deviceInfo = device.GetDeviceInfo();
            Console.WriteLine($"Sent data to device - {deviceInfo}");
        }

        await Task.Delay(2000, cancellation.Token);
    }
}
catch (OperationCanceledException)
{

}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    sensorMonitor.Dispose();
    Console.WriteLine("Rvl.Host.App has exited.");
    if (hasDevice)
    {
        await device.SendCommand(Constants.Report.Command.MessageCheckHost);
        device.Dispose();
    }
}


