namespace WpfApp1;

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

    private void HeavyOperation()
    {
        var rnd = new Random();
        Thread.Sleep(rnd.Next(1000, 8000));
    }
}
