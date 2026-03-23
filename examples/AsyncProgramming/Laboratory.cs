using System.Diagnostics;

namespace AsyncProgramming;

public class Laboratory
{
    public Laboratory()
    {
        var lines = File.ReadAllLines("Data/StockPrices_Small.csv");
        Data = new List<StockPrice>();
        foreach (var line in lines.Skip(1))
        {
            var price = StockPrice.FromCsv(line);
            Data.Add(price);
        }
    }

    private List<StockPrice> Data { get; }

    public void Lab1()
    {
        Console.WriteLine("Running HeavyOperation to understand it");
        HeavyOperation("Lab1");
    }


    private void HeavyOperation(string taskName, CancellationToken? cancellationToken = null)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.WriteLine($"Started {taskName} | Thread: {Environment.CurrentManagedThreadId}");
        Enumerable
            .Repeat(Data, 4000)
            .SelectMany(x => x)
            .ToList();
        stopwatch.Stop();
        Console.WriteLine($"Finished {taskName} in {stopwatch.Elapsed}");
    }
}
