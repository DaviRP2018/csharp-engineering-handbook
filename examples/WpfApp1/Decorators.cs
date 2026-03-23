using System.Diagnostics;

namespace WpfApp1;

[Obsolete("Adds a lot of complexity to provide async examples")]
public class ButtonDecorator
{
    private readonly Action<string> _logAction;

    public ButtonDecorator(Action<string> logAction)
    {
        _logAction = logAction;
    }

    public void Decorate(string jobName, Action action)
    {
        _logAction($"{jobName} - Started at {DateTime.Now:T}");

        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        _logAction($"{jobName} - Took {stopwatch.ElapsedMilliseconds}ms");
    }
}
