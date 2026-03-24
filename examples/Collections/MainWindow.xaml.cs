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

    public MainWindow()
    {
        InitializeComponent();
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
        GC.Collect();
        GC.WaitForPendingFinalizers();
        statusText.Text = "Status: Memory Cleared.";
    }
}
