using System.Data;
using Rvl.Display.Core;
using Terminal.Gui;
namespace Rvl.Display.Cli;

internal partial class CommandView : Window
{
    public const int ViewWidth = 98;
    public const int ViewHeight = 40;

    private View _infoView;
    private View _buttonContainerView;
    private Label _statusLabel;
    private TextView _messageLabel;
    private TableView _streamTableView;
    private DataTable _streamTable;
    private const int _ButtonWidth = 28;
    private const int _ButtonContainerViewHeight = 16;
    private readonly int _ButtonLayoutSize = Enum.GetValues<CommandCategory>().Length;

    private void InitializeComponent()
    {
        this.Title = $"{Constants.CliName} - {Application.QuitKey} to Exit";
        this.Width = ViewWidth;
        this.Height = ViewHeight;
        this.X = Pos.Center();
        this.Y = Pos.Center();
        this.BorderStyle = LineStyle.Heavy;

        _buttonContainerView = new View()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = _ButtonContainerViewHeight,
            CanFocus = true
        };

        for (int i = 0; i < _ButtonLayoutSize; i++)
        {
            _buttonContainerView.Add(new View()
            {
                X = i * (ViewWidth / _ButtonLayoutSize) + 1,
                Y = 1,
                Width = ViewWidth / _ButtonLayoutSize - 2,
                Height = Dim.Fill(2),
                BorderStyle = LineStyle.Dotted,
                Title = CommandCategory.GetName(typeof(CommandCategory), i),
                CanFocus = true,
                ColorScheme = new ColorScheme { Focus = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black) }
            });
        }

        Add(_buttonContainerView);

        Commands.List.ForEach(command =>
            {
                Button button = new()
                {
                    Text = command.Name,
                    Data = command,
                    X = 1,
                    Y = (command.Order - 1) * 2 + 1,
                    CanFocus = true,
                    ColorScheme = new ColorScheme { Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan) },
                };

                button.Accepting += (s, e) =>
                {
                    HandleButtonEvent(button, e);
                };
                _buttonContainerView.Subviews[command.Category].Add(button);
            });

        _infoView = new View
        {
            X = 1,
            Y = Pos.Bottom(_buttonContainerView) - 1,
            Width = Dim.Fill(4),
            Height = 6,
        };

        _statusLabel = new Label
        {
            Title = "Status",
            Text = "...",
            X = 1,
            Y = 1,
        };

        _messageLabel = new TextView
        {
            Title = "Message",
            Text = "...",
            X = 1,
            Y = Pos.Bottom(_statusLabel) + 1,
            ReadOnly = true,
            Height = 3,
            Width = Dim.Fill(2),
        };

        _infoView.Add(_statusLabel);
        _infoView.Add(_messageLabel);

        Add(_infoView);

        _streamTable = new DataTable();
        _streamTable.Columns.Add("  Sensor ");
        _streamTable.Columns.Add("    Load ");
        _streamTable.Columns.Add("    Temp ");
        _streamTable.Columns.Add("   Speed ");
        _streamTable.Rows.Add("     CPU ", " ", " ", " ");
        _streamTable.Rows.Add("     GPU ", " ", " ", " ");
        _streamTable.Rows.Add(" CHASSIS ", " ", " ", " ");

        _streamTableView = new TableView(new DataTableSource(_streamTable))
        {
            Title = "Sensor Stream",
            X = 1,
            Y = Pos.Bottom(_infoView),
            Width = Dim.Fill(4),
            Height = Dim.Fill(1),
            CanFocus = true,
            BorderStyle = LineStyle.Single
        };

        Add(_streamTableView);
    }

    protected override bool OnKeyDown(Key key)
    {
        // Terminal.Gui v2 uses the Key object directly
        if (key.KeyCode == KeyCode.CursorRight || key.KeyCode == KeyCode.CursorLeft)
        {
            var currentFocus = Application.Navigation.GetFocused();

            if (currentFocus is Button currentButton && currentButton.SuperView != null)
            {
                var currentColumn = currentButton.SuperView;
                var currentColumnIndex = _buttonContainerView.Subviews.IndexOf(currentColumn);

                int targetIndex = key.KeyCode == KeyCode.CursorRight ? currentColumnIndex + 1 : currentColumnIndex - 1;

                if (targetIndex >= 0 && targetIndex < _buttonContainerView.Subviews.Count)
                {
                    var targetColumn = _buttonContainerView.Subviews[targetIndex];

                    var targetButton = targetColumn.Subviews
                        .OfType<Button>()
                        .OrderBy(b => Math.Abs(b.Frame.Y - currentButton.Frame.Y))
                        .FirstOrDefault();

                    if (targetButton != null)
                    {
                        targetButton.SetFocus();
                        return true;
                    }
                }
            }
        }

        return base.OnKeyDown(key);
    }
}