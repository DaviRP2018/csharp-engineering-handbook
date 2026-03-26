using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Disposable;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DisposableMemory> DisposableItems { get; } = new();


    private void AllocateUnmanagedMemory_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var arg = (string)button.Tag;
        int.TryParse(arg, out var mBytes);
        new UnmanagedMemoryHandler().Leak(mBytes);
        UnmanagedStatusText.Text = $"Allocated {mBytes} MB";
    }

    private void ClearUnmanagedMemory_OnClick(object sender, RoutedEventArgs e)
    {
        UnmanagedMemoryHandler.FreeAll();
        UnmanagedStatusText.Text = "Memory cleared";
    }

    private void AllocateManagedMemory_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var arg = (string)button.Tag;
        int.TryParse(arg, out var mBytes);
        ManagedMemoryHandler.Allocate(mBytes);
        ManagedStatusText.Text = $"Allocated {mBytes} managed MB";
    }

    private void AllocateReferencedMemory_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var arg = (string)button.Tag;
        int.TryParse(arg, out var mBytes);
        ManagedMemoryHandler.Leak(mBytes);
        ManagedStatusText.Text =
            $"Allocated {mBytes} managed MB. But this will have a reference and won't be collected";
    }

    private void ClearManagedMemory_OnClick(object sender, RoutedEventArgs e)
    {
        ManagedMemoryHandler.Collect();
        ManagedStatusText.Text = "Asked GC to collect";
    }


    private void RemoveReferences_OnClick(object sender, RoutedEventArgs e)
    {
        ManagedMemoryHandler.RemoveReferences();
        ManagedStatusText.Text = "Removed references to that memory. GC can now collect";
    }

    private async void AllocateDisposable_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var arg = (string)button.Tag;
        int.TryParse(arg, out var mBytes);

        // using var handler = new DisposableMemory();  // C# 8+
        using (var handler = new DisposableMemory())
        {
            handler.Leak(mBytes);
            await Task.Delay(5000);
        }
    }
}

#region Unmanaged Memory Tab

public class UnmanagedMemoryHandler
{
    // We need to save the allocated pointers, otherwise if we don't know the pointer, there is
    // no way to manually free what we manually added.
    private static readonly List<IntPtr> Pointers = new();

    public void Leak(int mBytes)
    {
        var bytes = mBytes * 1024 * 1024;
        var ptr = Marshal.AllocHGlobal(bytes);
        Pointers.Add(ptr);

        Console.WriteLine($"Allocated {mBytes} MB at {ptr}");
    }

    public static void FreeAll()
    {
        foreach (var ptr in Pointers) Marshal.FreeHGlobal(ptr);

        Console.WriteLine("Memory cleared!");
        Pointers.Clear();
    }
}

#endregion

#region Managed Memory Tab

public static class ManagedMemoryHandler
{
    private static readonly List<byte[]> Memory = new();

    public static void Allocate(int mBytes)
    {
        var bytes = mBytes * 1024 * 1024;
        var data = new byte[bytes];

        // Force memory usage
        for (var i = 0; i < data.Length; i++) data[i] = 1;

        // The GC may consider this object (data) eligible for collection after its last use,
        // which might occur before the end of this method (if it was slow enough) due to JIT optimizations.
        Console.WriteLine($"Allocated managed {mBytes} MB.");
    }

    public static void Leak(int mBytes)
    {
        var bytes = mBytes * 1024 * 1024;
        var data = new byte[bytes];
        Memory.Add(data);

        // Here is one of the true dangers of managed memory: a reference (our List)
        // is keeping the data alive in memory. Because the List is static, it lives
        // for the entire lifetime of the application, preventing the GC from collecting this data.
        Console.WriteLine($"Holding {mBytes} MB. Total blocks: {Memory.Count}");
    }

    public static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public static void RemoveReferences()
    {
        Console.WriteLine("Clearing List data, so we remove references to data inside it");
        Memory.Clear();
    }
}

#endregion

#region IDisposable Tab

public class DisposableMemory : UnmanagedMemoryHandler, IDisposable
{
    public void Dispose()
    {
        FreeAll();
    }
}

#endregion
