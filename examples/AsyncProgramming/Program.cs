using System.Diagnostics;

namespace AsyncProgramming;

internal class Program
{
    public static async Task Main(string[] args)
    {
        // Console.WriteLine("Getting data...");
        //
        // CancellationTokenSource cancellationTokenSource = new();
        // cancellationTokenSource.Token.Register(() =>
        // {
        //     Console.WriteLine("I was cancelled");
        // });
        // await GetData(cancellationTokenSource.Token);
        //
        // Console.WriteLine("Got data");
        var lab = new Laboratory();
        lab.Lab1();
    }

    private static async Task GetData(CancellationToken cancellationToken)
    {
        var lines = File.ReadAllLines("Data/StockPrices_Small.csv");

        var data = new List<StockPrice>();

        foreach (var line in lines.Skip(1))
        {
            var price = StockPrice.FromCsv(line);
            data.Add(price);
        }

        Console.WriteLine("arf");
        var task1 = Task.Run(() => HeavyOperation(data, "Task 1", cancellationToken));
        var task2 = Task.Run(() => HeavyOperation(data, "Task 2", cancellationToken));
        var task3 = Task.Run(() => HeavyOperation(data, "Task 3", cancellationToken));
        var task4 = Task.Run(() => HeavyOperation(data, "Task 4", cancellationToken));
        Console.WriteLine("arf");
        await Task.WhenAll(task1, task2, task3, task4);
        /*
         * WaitAll
         *  Syncronous awaiting
         *  Blocks main thread
         *  Uses traditional block mecanism
         *  Doesn't cooperates with async/await
         *
         * WhenAll
         *  Assyncronous awaiting
         *  Doesn't blocks main thread
         *  Register continuation
         *  Cooperates with Task Scheduler
         *
         * Both waits, but in different maners
         */
        var task5 = Task.Run(() => HeavyOperation(data, "Task 5.1"), cancellationToken);
        var task6 = Task.Run(() => HeavyOperation(data, "Task 6.1"), cancellationToken);

        var task5Status = task5.Status;
        Console.WriteLine(task5Status);
        var task51 = task5.ContinueWith(_ => HeavyOperation(data, "Task 5.2"), cancellationToken);
        var task61 = task6.ContinueWith(_ => HeavyOperation(data, "Task 6.2", cancellationToken));
        await Task.WhenAll(task51, task61);
    }

    private static void HeavyOperation<T>(List<T> list, string taskName,
        CancellationToken? cancellationToken = null)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.WriteLine($"Started {taskName} | Thread: {Environment.CurrentManagedThreadId}");
        Enumerable
            .Repeat(list, 1000)
            .SelectMany(x => x)
            .ToList();
        stopwatch.Stop();
        Console.WriteLine($"Finished {taskName} in {stopwatch.Elapsed}");
    }
}
