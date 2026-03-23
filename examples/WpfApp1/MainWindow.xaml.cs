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

    private int _colorIndex;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void AddJob(string name)
    {
        JobsListBox.Items.Add($"Job: {name} (Started at {DateTime.Now:T})");
    }

    private void HeavyWorkButton_OnClick(object sender, RoutedEventArgs e)
    {
        AddJob("Heavy Work");
        StatusTextBlock.Text = "Status: Busy (UI is blocked!)";
        StatusTextBlock.Foreground = Brushes.Red;

        // Simulating heavy work that blocks the UI thread for 5 seconds
        Thread.Sleep(5000);

        StatusTextBlock.Text = "Status: Work Completed";
        StatusTextBlock.Foreground = Brushes.DarkGreen;
    }

    private void CycleColorButton_OnClick(object sender, RoutedEventArgs e)
    {
        AddJob("Cycle Color");
        _colorIndex = (_colorIndex + 1) % _colors.Length;
        MainProgressBar.Foreground = _colors[_colorIndex];
    }
}
