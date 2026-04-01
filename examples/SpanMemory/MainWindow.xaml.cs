using System.Windows;

namespace SpanMemory;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int ArraySize = 10_000_000; // 10 million elements
    private readonly List<int[]> _allocatedSlices = new();
    private readonly int[] _largeArray;

    public MainWindow()
    {
        InitializeComponent();
        _largeArray = new int[ArraySize];
        Log("Initialized a large array of 10,000,000 integers.");
        Log($"Memory consumed by array: {_largeArray.Length * sizeof(int) / 1024 / 1024} MB");
        Log("-------------------------------------------");
    }

    private void BtnTraditionalCopy_OnClick(object sender, RoutedEventArgs e)
    {
        Log("[Traditional Copy] Starting...");

        var beforeMemory = GC.GetTotalMemory(false);

        // Simulating a "slice" copy
        var slice = new int[ArraySize / 2];

        // Keep reference to prevent GC!
        _allocatedSlices.Add(slice);

        var afterMemory = GC.GetTotalMemory(false);
        var allocated = afterMemory - beforeMemory;

        Log($"[Traditional Copy] Copied {slice.Length:N0} elements.");
        Log(
            $"[Traditional Copy] Allocated: {allocated / 1024.0 / 1024.0:F2} MB (Total Slices: {_allocatedSlices.Count})");
        Log(
            $"[Traditional Copy] App Total Memory: {GC.GetTotalMemory(false) / 1024.0 / 1024.0:F2} MB");
        Log("-------------------------------------------");
    }

    private void BtnSpanSlice_OnClick(object sender, RoutedEventArgs e)
    {
        Log("[Span Slice] Starting...");

        var beforeMemory = GC.GetTotalMemory(false);

        // Zero-allocation slice
        ReadOnlySpan<int> slice = _largeArray.AsSpan(0, ArraySize / 2);

        var afterMemory = GC.GetTotalMemory(false);
        var allocated = afterMemory - beforeMemory;

        Log($"[Span Slice] Created span for {slice.Length:N0} elements.");
        Log($"[Span Slice] Allocated: {allocated / 1024.0 / 1024.0:F2} MB (Zero Copy!)");
        Log($"[Span Slice] App Total Memory: {GC.GetTotalMemory(false) / 1024.0 / 1024.0:F2} MB");
        Log("-------------------------------------------");

        // Just to ensure slice is not optimized away
        if (slice.Length > 0)
        {
            var _ = slice[0];
        }
    }

    private void BtnClearLog_OnClick(object sender, RoutedEventArgs e)
    {
        LogDisplay.Text = string.Empty;
        _allocatedSlices.Clear();
        GC.Collect();
        Log("Logs cleared, slice list emptied, and GC called.");
        Log("-------------------------------------------");
    }

    private void Log(string message)
    {
        LogDisplay.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        LogScrollViewer.ScrollToEnd();
    }
}
