using System.Collections;
using System.Diagnostics;
using System.Windows;

namespace Collections;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int ItemCount = 10_000_000;
    private const int LookupCount = 1000;
    private readonly Queue<string> _queue = new();
    private readonly Stack<string> _stack = new();
    private HashSet<int>? _cachedHashSet;
    private List<int>? _cachedList;
    private int _itemCounter = 1;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void EnsureLookupsReady()
    {
        if (_cachedList == null || _cachedHashSet == null)
        {
            _cachedList = new List<int>(ItemCount);
            _cachedHashSet = new HashSet<int>(ItemCount);
            for (var i = 0; i < ItemCount; i++)
            {
                _cachedList.Add(i);
                _cachedHashSet.Add(i);
            }
        }
    }

    private void btnListLookup_Click(object sender, RoutedEventArgs e)
    {
        EnsureLookupsReady();
        CommonLookupTask("List<int>", "O(n)", _cachedList!);
    }

    private void btnHashLookup_Click(object sender, RoutedEventArgs e)
    {
        EnsureLookupsReady();
        CommonLookupTask("HashSet<int>", "O(1)", _cachedHashSet!);
    }

    private void CommonLookupTask(string typeName, string complexity, ICollection<int> collection)
    {
        statusText.Text = $"Status: Running {LookupCount} lookups on {typeName} ({complexity})...";
        var sw = Stopwatch.StartNew();
        var foundCount = 0;

        // Look up the last item multiple times to show the worst case
        for (var i = 0; i < LookupCount; i++)
            if (collection.Contains(ItemCount - 1))
                foundCount++;

        sw.Stop();
        statusText.Text =
            $"Status: {typeName} lookup took {sw.ElapsedMilliseconds}ms for {foundCount} items.";
    }

    private void CommonTask<T>(string typeName, T list) where T : IList
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();

        statusText.Text = $"Status: Generating {typeName}...";
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < ItemCount; i++) list.Add(i); // Boxing happens here

        sw.Stop();
        statusText.Text =
            $"Status: {typeName} created in {sw.ElapsedMilliseconds}ms. Check memory!";
    }

    private void btnOld_Click(object sender, RoutedEventArgs e)
    {
        var list = new ArrayList(ItemCount);
        CommonTask("ArrayList (Boxed)", list);
    }

    private void btnNew_Click(object sender, RoutedEventArgs e)
    {
        var list = new List<int>(ItemCount);
        CommonTask("List<int>", list);
    }

    private void btnClear_Click(object sender, RoutedEventArgs e)
    {
        _cachedList = null;
        _cachedHashSet = null;
        _queue.Clear();
        _stack.Clear();
        _itemCounter = 1;
        UpdateFifoLifoDisplay();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        statusText.Text = "Status: Memory Cleared.";
    }

    private void btnEnqueue_Click(object sender, RoutedEventArgs e)
    {
        _queue.Enqueue($"Item {_itemCounter++}");
        UpdateFifoLifoDisplay();
    }

    private void btnDequeue_Click(object sender, RoutedEventArgs e)
    {
        if (_queue.Count > 0)
        {
            _queue.Dequeue();
            UpdateFifoLifoDisplay();
        }
    }

    private void btnPush_Click(object sender, RoutedEventArgs e)
    {
        _stack.Push($"Item {_itemCounter++}");
        UpdateFifoLifoDisplay();
    }

    private void btnPop_Click(object sender, RoutedEventArgs e)
    {
        if (_stack.Count > 0)
        {
            _stack.Pop();
            UpdateFifoLifoDisplay();
        }
    }

    private void UpdateFifoLifoDisplay()
    {
        listQueue.Items.Clear();
        foreach (var item in _queue) listQueue.Items.Add(item);

        listStack.Items.Clear();
        foreach (var item in _stack) listStack.Items.Add(item);
    }
}
