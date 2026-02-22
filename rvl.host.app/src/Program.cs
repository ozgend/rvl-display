using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rvl.Host.App.Core;
using Rvl.Host.App.Core.Hardware;
using Rvl.Host.App.Core.Host;
using Rvl.Host.App.Core.Interfaces;
using Rvl.Host.App.Core.Model;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.AddOptions<RvlDisplayConfigOptions>().Bind(builder.Configuration.GetRequiredSection(Constants.ServiceName)).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"{Constants.ServiceName}.Service";
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddEventLog();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddSingleton<IRvlDevice, RvlDevice>();
builder.Services.AddSingleton<IRvlSensorMonitor, RvlSensorMonitor>();
builder.Services.AddSingleton<IHidReportSink, HidReportSink>();
builder.Services.AddSingleton<IPipeForwardingServer, PipeForwardingServer>();
builder.Services.AddHostedService<RvlDisplayWorker>();

var host = builder.Build();
await host.RunAsync();