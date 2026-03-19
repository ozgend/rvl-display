using Rvl.Display.Cli;
using Terminal.Gui;

ConfigurationManager.RuntimeConfig = """{ "Theme": "Dark" }""";
Console.WindowHeight = CommandView.ViewHeight + 2;
Console.WindowWidth = CommandView.ViewWidth + 2;
Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;
Application.ForceDriver = "NetDriver";
Application.Init();

using var commandView = new CommandView();
commandView.Added += async (s, e) =>
{
    await commandView.ConnectToPipeAsync();
};
Application.Run(commandView);
