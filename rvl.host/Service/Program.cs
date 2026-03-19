using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;
using Rvl.Display.Core.Services;
using Rvl.Display.Service.Host;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.AddOptions<RvlDisplayConfigOptions>().Bind(builder.Configuration.GetRequiredSection(Constants.ServiceName)).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddWindowsService(options => { options.ServiceName = Constants.ServiceName; });

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddEventLog();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddSingleton<IRvlDevice, RvlDevice>();
builder.Services.AddSingleton<IRvlSensorMonitor, RvlSensorMonitor>();
builder.Services.AddSingleton<IRvlHidReportSink, RvlHidReportSink>();
builder.Services.AddSingleton<IRvlPipeServer, RvlPipeServer>();
builder.Services.AddHostedService<RvlDisplayService>();

var host = builder.Build();
await host.RunAsync();