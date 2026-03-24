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
    private CancellationTokenSource _cancellationTokenSource = new();

    private int _colorIndex;
    private int _jobId;

    private Task? _lastTask;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void PrintToMessageLog(string message)
    {
        var item = message;
        JobsListBox.Items.Add(item);
        JobsListBox.ScrollIntoView(item);
    }

    private void ClearMessageLog_OnClick(object sender, RoutedEventArgs e)
    {
        JobsListBox.Items.Clear();
    }

    private void ShowLastTaskStatus_OnClick(object sender, RoutedEventArgs e)
    {
        var message = "";
        message += $"Id: {_lastTask?.Id}\n";
        message += $"Status: {_lastTask?.Status}\n";
        message += $"Exception: {_lastTask?.Exception}\n";
        message += $"IsCanceled: {_lastTask?.IsCanceled}\n";
        message += $"IsCompleted: {_lastTask?.IsCompleted}\n";
        message += $"IsCompletedSuccessfully: {_lastTask?.IsCompletedSuccessfully}\n";
        message += $"IsFaulted: {_lastTask?.IsFaulted}";
        PrintToMessageLog(message);
    }

    private async void HeavyWorkButton_OnClick(object sender, RoutedEventArgs e)
    {
        _jobId++;
        var jobName = $"Heavy Work #{_jobId}";
        PrintToMessageLog($"{jobName} - Started at {DateTime.Now:T}");
        var stopwatch = Stopwatch.StartNew();


        StatusTextBlock.Text = "Status: Busy";
        StatusTextBlock.Foreground = Brushes.Red;

        #region Lab 01 Blocking Thread ============================================================

        // _lab.Lab_01_BlockingWork();

        #endregion

        #region Lab 02 Non Blocking Good Async ====================================================

        // _lastTask = _lab.Lab_02_NonBlockingWork();
        // await _lastTask;

        #endregion

        #region Lab 03 Using Task, but blocking ===================================================

        // _lastTask = _lab.Lab_03_BlockingWaitAll();
        // await _lastTask;

        #endregion

        #region Lab 04 Non Blocking When All ======================================================

        // _lastTask = _lab.Lab_04_NonBlockingWhenAll();
        // await _lastTask;

        #endregion

        #region Lab 05 await usage ================================================================

        // Using `Result` at the wrong time
        // var stuffFromDb = _lab.Lab_05_GetStuffFromDbAsync().Result;
        // // Many other things that could be done while getting stuff from DB...
        // PrintToScreen("Hello?");
        // // Finished stuff, now I really need that stuff from DB
        // PrintToScreen($"Using '{stuffFromDb}' to do something.");

        // Bad practice - Using await at the wrong time
        // var stuffFromDb = await _lab.Lab_05_GetStuffFromDbAsync();
        // // Many other things that could be done while getting stuff from DB...
        // PrintToScreen("I could be doing things meanwhile... but");
        // // Finished stuff, now I really need that stuff from DB
        // PrintToScreen($"Using '{stuffFromDb}' to do something.");

        // Good practice - Putting the task in a variable
        // var taskStuffFromDb = _lab.Lab_05_GetStuffFromDbAsync();
        // // Many other things that could be done while getting stuff from DB...
        // PrintToScreen("Doing things meanwhile!");
        // // Finished stuff, now I really need that stuff from DB
        // var stuffFromDb = await taskStuffFromDb;
        // PrintToScreen($"Using '{stuffFromDb}' to do something.");

        #endregion

        #region Lab 06 Async void =================================================================

        // What happens if we run a async void method?
        // _lab.Lab_06_AsyncVoidException();
        // try
        // {
        //     _lab.Lab_06_AsyncVoidException();
        // }
        // catch (Exception err)
        // {
        //     Console.WriteLine($"Ho ho ho! Gotcha pesky bug: {err}");
        // }

        #endregion

        #region Lab 07 Not awaiting Task & Exception handling it ==================================

        // What happens if we don't await a task? Will it execute in full "somewhere"?
        // _lastTask = _lab.Lab_07_AsyncTaskException();
        /*
         * calling an async Task method without awaiting it. This creates what's called a
         * "fire-and-forget" scenario. Here's what happens:
         *   The method starts executing on a background thread
         *   The calling method continues immediately without waiting
         *   The Task object is created but ignored - it goes out of scope
         *   Any exceptions occur asynchronously in the background
         *
         */

        // What if we try to catch the exception?
        // try
        // {
        //     _lastTask = _lab.Lab_07_AsyncTaskException(); // This doesn't throw here!
        //     // The `try-catch` block only catches exceptions that occur **synchronously** during
        //     // the method call. Since `Lab_06_AsyncTaskException()` returns immediately with a
        //     // `Task` object, no exception is thrown at this point. The actual exception happens
        //     // later, asynchronously, when the task executes this line:
        // }
        // catch (Exception err)
        // {
        //     Console.WriteLine(err); // This will never execute
        // }

        #endregion

        #region Lab 08 Catching an exception ======================================================

        // try
        // {
        //     _lastTask = _lab.Lab_08_ExceptionHandling();
        //     await _lastTask;
        // }
        // catch (InvalidOperationException err)
        // {
        //     PrintToMessageLog(err.ToString());
        // }

        // Handling multiple tasks
        // var task1 = _lab.Lab_02_NonBlockingWork();
        // var task2 = _lab.Lab_07_AsyncTaskException();
        // var task3 = _lab.Lab_05_GetStuffFromDbAsync();
        // var task4 = _lab.Lab_08_ExceptionHandling();
        //
        // // var tasks = new[] { task1, task2, task3, task4 };
        // var tasksDict = new Dictionary<string, Task>
        // {
        //     { "Cooking rice", task1 },
        //     { "Preparing meat", task2 },
        //     { "Cleaning plates", task3 },
        //     { "Seasoning", task4 },
        // };
        //
        // try
        // {
        //     await Task.WhenAll(tasksDict.Values);
        // }
        // catch
        // {
        //     foreach (var (taskName, task) in tasksDict)
        //     {
        //         if (task.IsFaulted)
        //         {
        //             foreach (var ex in task.Exception!.InnerExceptions)
        //             {
        //                 PrintToMessageLog($"Task '{taskName}' failed: {ex.Message}");
        //             }
        //         }
        //     }
        // }

        #endregion

        #region Lab 09 Trying to use Cancellation Token ===========================================

        // try
        // {
        //     if (_cancellationTokenSource.IsCancellationRequested)
        //     {
        //         _cancellationTokenSource.Dispose();
        //         _cancellationTokenSource = new CancellationTokenSource();
        //     }
        //
        //     _lastTask = _lab.Lab_09_CancellationToken(_cancellationTokenSource.Token);
        //     await _lastTask;
        // }
        // catch (OperationCanceledException)
        // {
        //     PrintToMessageLog("Task was cancelled");
        // }
        // catch (Exception err)
        // {
        //     PrintToMessageLog($"Error: {err.Message}");
        // }

        #endregion

        #region Lab 10 Handling multiple tasks ====================================================

        // Funny multiple tasks with WhenAny ======================================================
        // var sentence = "";
        // var tasks = new List<Task<string>>
        // {
        //     _lab.Lab_10_GetSubjectAsync(),
        //     _lab.Lab_10_GetVerbAsync(),
        //     _lab.Lab_10_GetObjectAsync()
        // };
        //
        // var sentence = "";
        //
        // while (tasks.Any())
        // {
        //     var finishedTask = await Task.WhenAny(tasks);
        //     sentence += finishedTask.Result + " ";
        //     tasks.Remove(finishedTask);
        // }

        // ContinueWith ===========================================================================
        // var subjectTask = _lab.Lab_10_GetSubjectAsync();
        //
        // var verbTask = subjectTask.ContinueWith(t =>
        //     _lab.Lab_10_GetVerbAsync()
        // ).Unwrap();
        //
        // var objectTask = verbTask.ContinueWith(t =>
        //     _lab.Lab_10_GetObjectAsync()
        // ).Unwrap();
        //
        // var subject = await subjectTask;
        // var verb = await verbTask;
        // var obj = await objectTask;
        //
        // var sentence = $"{subject} {verb} {obj}";


        // When any, think of multiple API calls to check what is fastest =========================
        var subject = _lab.Lab_10_GetSubjectAsync();
        var verb = _lab.Lab_10_GetVerbAsync();
        var obj = _lab.Lab_10_GetObjectAsync();

        var first = await Task.WhenAny(subject, verb, obj);
        var sentence = await first;

        PrintToMessageLog(sentence);

        #endregion


        StatusTextBlock.Text = "Status: Work Completed";
        StatusTextBlock.Foreground = Brushes.DarkGreen;

        stopwatch.Stop();
        PrintToMessageLog(
            $"{jobName} - Took {stopwatch.ElapsedMilliseconds}ms - Finished at {DateTime.Now:T}");
    }

    private void CancelTask_OnClick(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource.Cancel();
    }

    private void CycleColorButton_OnClick(object sender, RoutedEventArgs e)
    {
        var newColor = _colors[_colorIndex];
        var brush = (SolidColorBrush)newColor;
        var c = brush.Color;

        var name = typeof(Colors)
            .GetProperties()
            .FirstOrDefault(p => (Color)p.GetValue(null) == c)?.Name;

        PrintToMessageLog($"Cycle color: {name ?? c.ToString()}");
        _colorIndex = (_colorIndex + 1) % _colors.Length;
        MainProgressBar.Foreground = newColor;
    }
}
