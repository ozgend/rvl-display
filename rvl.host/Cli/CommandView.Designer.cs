
using Terminal.Gui;
namespace Rvl.Display.Cli;

internal partial class CommandView : Window
{
    private View _infoView;
    private Label _statusLabel;
    private Label _messageLabel;
    private const int _ButtonWidth = 32;

    private void InitializeComponent()
    {
        _infoView = new View();
        _infoView.X = 1;
        _infoView.Y = 1;
        _infoView.Padding.SetAttribute(new Terminal.Gui.Attribute(1, 1));
        _infoView.Width = Dim.Fill(2);
        _infoView.Height = 5;
        _infoView.Title = "Information";

        _statusLabel = new Label();
        _statusLabel.Text = "...";
        _statusLabel.Title = "Status";
        _statusLabel.X = 1;
        _statusLabel.Y = 1;
        _infoView.Add(_statusLabel);

        _messageLabel = new Label();
        _messageLabel.Text = "Select a command:";
        _messageLabel.X = 1;
        _messageLabel.Y = Pos.Top(_statusLabel) + 2;
        _infoView.Add(_messageLabel);

        Add(_infoView);

        var line = new Line
        {
            X = 1,
            Y = Pos.Bottom(_infoView) + 1,
            Width = Dim.Fill(1)
        };
        Add(line);

        var buttonStartX = 2;
        var buttonStartY = Pos.Bottom(line) + 1;

        Commands.List.ForEach(command =>
        {
            Button button = new()
            {
                Text = command.Name,
                Data = command,
                X = buttonStartX + ((command.Category - 1) * _ButtonWidth),
                Y = buttonStartY + ((command.Order - 1) * 2),
                // HotKey = command.HotKey,
            };

            button.Accepting += (s, e) =>
            {
                HandleButtonEvent(button, e);
                // statusLabel.Text = $"Accept: {command.Name}";
            };

            button.MouseClick += (s, e) =>
            {
                HandleButtonEvent(button, e);
                // statusLabel.Text = $"Click {command.Name}";
                e.Handled = true;
            };

            Add(button);
        });
    }
}