using System.Windows;
using System.Windows.Media;

namespace WpfApp1;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Brush[] _colors =
        { Brushes.Green, Brushes.Red, Brushes.Blue, Brushes.Yellow };

    private readonly ButtonDecorator _decorator;

    private int _colorIndex;

    public MainWindow()
    {
        InitializeComponent();
        _decorator = new ButtonDecorator(msg => JobsListBox.Items.Add(msg));
    }


    private void HeavyWorkButton_OnClick(object sender, RoutedEventArgs e)
    {
        _decorator.Decorate("Heavy Work", () =>
        {
            StatusTextBlock.Text = "Status: Busy (UI is blocked!)";
            StatusTextBlock.Foreground = Brushes.Red;

            // Simulating heavy work that blocks the UI thread for 5 seconds
            Thread.Sleep(5000);

            StatusTextBlock.Text = "Status: Work Completed";
            StatusTextBlock.Foreground = Brushes.DarkGreen;
        });
    }

    private void CycleColorButton_OnClick(object sender, RoutedEventArgs e)
    {
        _decorator.Decorate("Cycle Color", () =>
        {
            _colorIndex = (_colorIndex + 1) % _colors.Length;
            MainProgressBar.Foreground = _colors[_colorIndex];
        });
    }
}
