using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Collections;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int ItemCount = 10_000_000;
    private const int LookupCount = 1000;
    private readonly List<object> _boxedCommonList = new();
    private readonly Dictionary<int, object> _immutableItemCache = new();
    private readonly Queue<string> _queue = new();
    private readonly Stack<string> _stack = new();
    private HashSet<int>? _cachedHashSet;
    private List<int>? _cachedList;
    private readonly List<int> _commonList = new();
    private ImmutableList<int> _immutableList = ImmutableList<int>.Empty;
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
        _commonList.Clear();
        _boxedCommonList.Clear();
        _immutableItemCache.Clear();
        _immutableList = ImmutableList<int>.Empty;
        _itemCounter = 1;
        UpdateFifoLifoDisplay();
        UpdateImmutableDisplay();
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

    private void btnAddToList_Click(object sender, RoutedEventArgs e)
    {
        var val = _itemCounter++;
        _commonList.Add(val);
        _boxedCommonList.Add(val); // Box it once to keep the same reference
        UpdateImmutableDisplay();
    }

    private void btnCopyList_Click(object sender, RoutedEventArgs e)
    {
        var copy = new List<int>(_commonList);
        // Create NEW boxed objects for the copied list to demonstrate that it's a NEW set of items in memory (for value types)
        var boxedCopy = copy.Select(i => (object)i).ToList();

        statusText.Text =
            $"Status: List copied. Original: {GetAddress(_commonList)}, Copy: {GetAddress(copy)} (New instance, full copy)";

        // Show the copied list in the UI to confirm it has DIFFERENT item addresses and DIFFERENT list address
        listCommon.Items.Clear();
        for (var i = 0; i < copy.Count; i++)
            listCommon.Items.Add(new { Value = copy[i], Address = GetAddress(boxedCopy[i]) });

        listAddressLabel.Text = $"Address (COPY): {GetAddress(copy)}";
    }

    private void btnAddImmutable_Click(object sender, RoutedEventArgs e)
    {
        var oldAddress = GetAddress(_immutableList);
        var val = _itemCounter++;

        // Cache the boxed value to show it's the SAME object in different list instances
        if (!_immutableItemCache.ContainsKey(val)) _immutableItemCache[val] = val;

        _immutableList = _immutableList.Add(val);
        var newAddress = GetAddress(_immutableList);
        statusText.Text =
            $"Status: Immutable item added. Old: {oldAddress}, New: {newAddress} (New instance, shared data)";
        UpdateImmutableDisplay();
    }

    private void btnCopyImmutable_Click(object sender, RoutedEventArgs e)
    {
        var copy = _immutableList; // Assignment just copies the reference
        statusText.Text =
            $"Status: Immutable assigned. Original: {GetAddress(_immutableList)}, Copy: {GetAddress(copy)} (Same instance)";

        // Refresh the display with the "copy" (which is the same object)
        UpdateImmutableDisplay();
        immutableAddressLabel.Text = $"Address (REF): {GetAddress(copy)}";
    }

    private void UpdateImmutableDisplay()
    {
        listCommon.Items.Clear();
        for (var i = 0; i < _commonList.Count; i++)
            listCommon.Items.Add(new
                { Value = _commonList[i], Address = GetAddress(_boxedCommonList[i]) });

        listAddressLabel.Text = $"Address: {GetAddress(_commonList)}";

        listImmutable.Items.Clear();
        foreach (var item in _immutableList)
        {
            // Use cached boxed version to show stability of item addresses
            var boxed = _immutableItemCache.TryGetValue(item, out var b) ? b : item;
            listImmutable.Items.Add(new { Value = item, Address = GetAddress(boxed) });
        }

        immutableAddressLabel.Text = $"Address: {GetAddress(_immutableList)}";
    }

    private string GetAddress(object obj)
    {
        if (obj == null) return "null";
        // GCHandle.ToIntPtr(handle) on a Weak handle returns the address of the handle itself, 
        // which might be reused. To show object IDENTITY in a way that looks like an address,
        // we'll use a trick that is more stable for this demonstration.
        // For educational purposes, a fixed hash code or a stable ID is better.
        return $"0x{RuntimeHelpers.GetHashCode(obj):X8}";
    }
}
