using Rvl.Display.Cli;
using Rvl.Display.Core;
using Terminal.Gui;

ConfigurationManager.RuntimeConfig = """{ "Theme": "Dark" }""";

Application.Init();

using var commandView = new CommandView()
{
    Title = $"{Constants.CliName} - {Application.QuitKey} to Exit",
    Width = 128,
    Height = 32
};

commandView.Added += async (s, e) =>
{
    await commandView.ConnectToPipeAsync();
};

Application.Run(commandView);