using Rvl.Host.App;

var commandMap = new Dictionary<(ConsoleKey, ConsoleModifiers), (RvlCommandData, string)>
{
    { (ConsoleKey.D1, ConsoleModifiers.Control), (new RvlCommandData(Constants.Report.Command.MessageClear), "MessageClear") },
    { (ConsoleKey.D2, ConsoleModifiers.Control), (new RvlCommandData(Constants.Report.Command.MessageAwait), "MessageAwait") },
    { (ConsoleKey.D3, ConsoleModifiers.Control), (new RvlCommandData(Constants.Report.Command.MessageCheckHost), "MessageCheckHost") },
    { (ConsoleKey.D4, ConsoleModifiers.Control), (new RvlCommandData(Constants.Report.Command.MessageHighTemp), "MessageHighTemp") },
    { (ConsoleKey.D5, ConsoleModifiers.Control), (new RvlCommandData(Constants.Report.Command.MessageLowRpm), "MessageLowRpm") },
    { (ConsoleKey.F, ConsoleModifiers.Alt), (new RvlCommandData(Constants.Report.Command.EnterBootloader), "EnterBootloader") },
    { (ConsoleKey.R, ConsoleModifiers.Alt), (new RvlCommandData(Constants.Report.Command.RestartDevice), "RestartDevice") },
    { (ConsoleKey.C, ConsoleModifiers.None), (new RvlCommandData(Constants.Report.Command.ClearDisplay), "ClearDisplay") },
    { (ConsoleKey.B, ConsoleModifiers.None), (new RvlCommandData(Constants.Report.Command.SetBrightnessHigh), "SetBrightnessHigh") },
    { (ConsoleKey.N, ConsoleModifiers.None), (new RvlCommandData(Constants.Report.Command.SetBrightnessMedium), "SetBrightnessMedium") },
    { (ConsoleKey.M, ConsoleModifiers.None), (new RvlCommandData(Constants.Report.Command.SetBrightnessLow), "SetBrightnessLow") },
    { (ConsoleKey.O, ConsoleModifiers.None), (new RvlCommandData(Constants.Report.Command.LedOn), "LedOn") },
    { (ConsoleKey.P, ConsoleModifiers.None), (new RvlCommandData(Constants.Report.Command.LedOff), "LedOff") },
};

var device = new RvlDevice();
var deviceEventHandler = new Action<string>(async eventName =>
{
    Console.WriteLine($"Main: Device event: {eventName}");
    switch (eventName)
    {
        case Constants.Events.DeviceConnected:
            Console.WriteLine("RvlDevice connected.");
            await device.Send(new RvlCommandData(Constants.Report.Command.MessageClear));
            break;
        default:
            break;
    }
});

device.Initialize(deviceEventHandler);

var sensorMonitor = new RvlSensorMonitor();
var canMonitor = sensorMonitor.Initialize();

if (!canMonitor)
{
    Console.WriteLine("Error: Failed to initialize monitor.");
    if (device.IsConnected)
    {
        await device.Send(new RvlCommandData(Constants.Report.Command.MessageCheckHost));
        device.Dispose();
    }
    Environment.Exit(1);
}

using var cancellation = new CancellationTokenSource();

Console.CancelKeyPress += async (sender, e) =>
{
    if (device.IsConnected)
    {
        await device.Send(new RvlCommandData(Constants.Report.Command.MessageCheckHost));
    }
    Console.WriteLine("Ctrl+C received. Exiting...");
    e.Cancel = true;
    cancellation.Cancel();
};

if (!System.Diagnostics.Debugger.IsAttached)
{
    Console.Clear();
    Console.CursorVisible = false;
}

Console.WriteLine("Rvl.Host.App | exit=[CTRL+C] | bootloader=[F] | ledON=[O] | ledOFF=[P] | brightness=[B/M/N] | clear=[C] | restart=[R] | message=[CTRL+1..5]");

if (device.IsConnected)
{
    await device.Send(new RvlCommandData(Constants.Report.Command.LedOff));
    await device.Send(new RvlCommandData(Constants.Report.Command.MessageClear));
}

try
{
    while (!cancellation.Token.IsCancellationRequested)
    {
        // cli input handling
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            if (device.IsConnected && Console.KeyAvailable)
            {
                var mappedCommand = commandMap.FirstOrDefault(kvp => kvp.Key.Item1 == Console.ReadKey(true).Key
                    && (kvp.Key.Item2 == ConsoleModifiers.None
                    || (ConsoleModifiers.Control & ConsoleModifiers.Control) != 0)
                ).Value;

                if (mappedCommand.Item1 != null)
                {
                    await device.Send(mappedCommand.Item1);
                    Console.WriteLine($"RvlCommand sent {mappedCommand.Item2}:{mappedCommand.Item1.Command:X2}");
                }
            }
        }

        var data = sensorMonitor.Poll();

        // Console.SetCursorPosition(startCol, startRow);
        // Console.WriteLine($"CPU: {data.CpuName,-16} - {data.CpuTemperature,4:F1}°C, {data.CpuUtilization,5:F1}%   ");
        // Console.WriteLine($"GPU: {data.GpuName,-16} - {data.GpuTemperature,4:F1}°C, {data.GpuUtilization,5:F1}%   ");

        if (device.IsConnected)
        {
            await device.Send(data);
        }

        await Task.Delay(Constants.SensorPollIntervalMs, cancellation.Token);
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
    if (device.IsConnected)
    {
        await device.Send(new RvlCommandData(Constants.Report.Command.MessageCheckHost));
        device.Dispose();
    }

    sensorMonitor.Dispose();
    Console.WriteLine("Rvl.Host.App has exited.");
}


