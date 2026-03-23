namespace Delegates;

public class OrderProcessor
{
    // public delegate bool ProduceShippingLabel(Order order);
    // public ProduceShippingLabel? OnProduceShippingLabel { get; set; }

    public Func<Order, bool>? OnProduceShippingLabel { get; set; }

    public event EventHandler? OrderCreated;

    protected virtual void OnOrderCreated()
    {
        Console.WriteLine($"[PUBLISHER]: I'm the only one who can invoke the event ({this})");
        OrderCreated?.Invoke(this, EventArgs.Empty);
    }

    private void Initialize(Order order)
    {
        OnOrderCreated();
    }

    public void Process(Order order)
    {
        // Run some code...

        Initialize(order);

        // How do I produce a shipping label?
        OnProduceShippingLabel?.Invoke(order);
    }
}

public class BatchOrderProcessor : OrderProcessor
{
    protected override void OnOrderCreated()
    {
        Console.WriteLine(
            $"[SUBSCRIBER]: Order was created, so I was called to do some work before passing to the base event ({this})");
        Thread.Sleep(3000);
        base.OnOrderCreated();
    }
}
