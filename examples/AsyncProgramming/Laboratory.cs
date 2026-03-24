using System.Diagnostics;

namespace AsyncProgramming;

public class Laboratory
{
    public void Lab_01_BlockingWork()
    {
        HeavyOperation();
    }

    public async Task Lab_02_NonBlockingWork()
    {
        await Task.Run(HeavyOperation);
    }

    public async Task Lab_03_BlockingWaitAll()
    {
        var task = Task.Run(HeavyOperation);
        Task.WaitAll(task);
        /*
         * WaitAll
         *  Synchronous awaiting
         *  Blocks main thread
         *  Uses traditional block mechanism
         *  Doesn't cooperate with async/await
         *
         * WhenAll
         *  Asynchronous awaiting
         *  Doesn't block main thread
         *  Register continuation
         *  Cooperates with Task Scheduler
         *
         * Both waits, but in different manners
         */
    }

    public async Task Lab_04_NonBlockingWhenAll()
    {
        await Task.Run(HeavyOperation);
    }

    public async Task<string> Lab_05_GetStuffFromDbAsync()
    {
        await Task.Delay(3000);
        return "Stuff from DB";
    }

    public async void Lab_06_AsyncVoidException()
    {
        // "Fire and forget" pattern.
        // DANGEROUS: Exceptions will crash the process if not caught in the async void method.
        // It's also hard to test and you can't await it.
        await Task.Run(HeavyOperation);
        throw new Exception("[Daemonic Bug] This will crash the app if unhandled");
    }

    /// <summary>
    ///     Explains why the application does not crash when exceptions occur in unawaited Tasks.
    /// </summary>
    /// <remarks>
    ///     Unlike <c>async void</c> methods (which can crash the application due to unhandled
    ///     exceptions),
    ///     unhandled exceptions in unawaited <c>Task</c> instances are handled differently by the
    ///     runtime.
    /// </remarks>
    /// <para>
    ///     The exception lifecycle in an unawaited Task follows these steps:
    /// </para>
    /// <list type="number">
    ///     <item>
    ///         <description>The exception is captured in the Task's internal state.</description>
    ///     </item>
    ///     <item>
    ///         <description>The Task is marked as faulted (<c>task.Status == TaskStatus.Faulted</c>).</description>
    ///     </item>
    ///     <item>
    ///         <description>The exception is stored in the <c>task.Exception</c> property.</description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             No crash occurs because the exception remains managed by the Task
    ///             infrastructure.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para>
    ///     Since no code is awaiting the Task, the .NET runtime does not propagate the exception,
    ///     effectively preventing it from crashing the application.
    /// </para>
    public async Task Lab_07_AsyncTaskException()
    {
        // Did you notice we are not returning anything like void?
        await Task.Run(HeavyOperation);

        string response = null;
        var count = response.Length;
    }

    public async Task Lab_08_ExceptionHandling()
    {
        await Task.Run(() =>
        {
            Thread.Sleep(6000);
            throw new InvalidOperationException("Something went wrong");
        });
    }

    public async Task Lab_09_CancellationToken(CancellationToken cancellationToken)
    {
        try
        {
            await HeavyOperationAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Operation was cancelled in Lab_08");
            throw;
        }
    }

    public async Task<string> Lab_10_GetSubjectAsync()
    {
        await Task.Delay(Random.Shared.Next(100, 1500));
        Console.WriteLine("Completed cat");
        return "the cat";
    }

    public async Task<string> Lab_10_GetVerbAsync()
    {
        await Task.Delay(Random.Shared.Next(100, 1500));
        Console.WriteLine("Completed eat");
        return "eats";
    }

    public async Task<string> Lab_10_GetObjectAsync()
    {
        await Task.Delay(Random.Shared.Next(100, 1500));
        Console.WriteLine("Completed pizza");
        return "pizza";
    }

    private void HeavyOperation()
    {
        var rnd = new Random();
        Thread.Sleep(rnd.Next(1000, 8000));
    }

    private async Task HeavyOperationAsync(CancellationToken cancellationToken)
    {
        var rnd = new Random();
        await Task.Delay(rnd.Next(1000, 8000), cancellationToken);
    }
}
