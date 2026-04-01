using System.Windows;

namespace ConfigureAwaitDeadlock;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var result = GetDataAsync().Result; // Deadlock
        MessageBox.Show(result);
    }

    public async Task<string> GetDataAsync()
    {
        Console.WriteLine("Hello there");

        // await Task.Delay(1000).ConfigureAwait(true);  // Captures UI context: deadlock
        await Task.Delay(1000).ConfigureAwait(false); // Can return to any thread: no deadlock

        Console.WriteLine("General Kenobi!"); // Won't print
        return "Done";
    }
}
