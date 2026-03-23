using System.Diagnostics;
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

    private readonly Laboratory _lab = new();

    private int _colorIndex;
    private int _jobId;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void AddJobAndScroll(string message)
    {
        var item = message;
        JobsListBox.Items.Add(item);
        JobsListBox.ScrollIntoView(item);
    }

    private async void HeavyWorkButton_OnClick(object sender, RoutedEventArgs e)
    {
        _jobId++;
        var jobName = $"Heavy Work #{_jobId}";
        AddJobAndScroll($"{jobName} - Started at {DateTime.Now:T}");
        var stopwatch = Stopwatch.StartNew();


        StatusTextBlock.Text = "Status: Busy";
        StatusTextBlock.Foreground = Brushes.Red;

        // Simulating heavy work that blocks the UI thread for random seconds
        // var task = Task.Run(_lab.Lab1);
        // await Task.WhenAll(task);
        // _lab.Lab_01_BlockingWork();

        var task = _lab.Lab_02_NonBlockingWork();
        await task;


        StatusTextBlock.Text = "Status: Work Completed";
        StatusTextBlock.Foreground = Brushes.DarkGreen;

        stopwatch.Stop();
        AddJobAndScroll(
            $"{jobName} - Took {stopwatch.ElapsedMilliseconds}ms - Finished at {DateTime.Now:T}");
    }

    private void CycleColorButton_OnClick(object sender, RoutedEventArgs e)
    {
        var newColor = _colors[_colorIndex];
        var brush = (SolidColorBrush)newColor;
        var c = brush.Color;

        var name = typeof(Colors)
            .GetProperties()
            .FirstOrDefault(p => (Color)p.GetValue(null) == c)?.Name;

        AddJobAndScroll($"Cycle color: {name ?? c.ToString()}");
        _colorIndex = (_colorIndex + 1) % _colors.Length;
        MainProgressBar.Foreground = newColor;
    }
}
