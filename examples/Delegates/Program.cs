using System.Reflection;
using Delegates;
using Delegates.Domain;

Order[] orders = new[]
{
    new Order
    {
        OrderNumber = Guid.NewGuid(),
    },
    new Order
    {
        OrderNumber = Guid.NewGuid(),
    },
    new Order
    {
        OrderNumber = Guid.NewGuid(),
    }
};

var orderProcessor = new OrderProcessor();

bool BlueLabelShipment(Order order)
{
    Console.WriteLine($"I'm labeling this order ({order.OrderNumber}) package!");
    return true;
}

bool GoldenLabelShipment(Order order)
{
    Console.WriteLine($"Labeling golden label for order! ({order.OrderNumber})");
    return true;
}

bool AddMoreLabels(Order order)
{
    Console.WriteLine(
        $"Adding more unnecessary labels for order {order.OrderNumber} to confuse people!");
    return true;
}


orderProcessor.OnProduceShippingLabel = GoldenLabelShipment;
Console.WriteLine(orderProcessor.OnProduceShippingLabel.GetMethodInfo());
orderProcessor.OnProduceShippingLabel += AddMoreLabels;
Console.WriteLine(orderProcessor.OnProduceShippingLabel.GetMethodInfo());
Console.WriteLine(orderProcessor.OnProduceShippingLabel.GetInvocationList());
foreach (var met in orderProcessor.OnProduceShippingLabel.GetInvocationList())
{
    Console.WriteLine(met.Method);
}

foreach (var order in orders)
{
    orderProcessor.Process(order);
}

orderProcessor.OnProduceShippingLabel = null;
Console.WriteLine(orderProcessor.OnProduceShippingLabel?.GetMethodInfo());

foreach (var order in orders)
{
    orderProcessor.Process(order);
}

// OrderProcessor.ProduceShippingLabel chain = BlueLabelShipment;
var chain = BlueLabelShipment;
chain += AddMoreLabels;

orderProcessor.OnProduceShippingLabel = chain;

// ================================================================================================
// Events

orderProcessor.OrderCreated += (sender, eventArgs) =>
{
    Console.WriteLine($"[SUBSCRIBER]: I'm doing some work ({sender})");
    Thread.Sleep(5000);
};


void Log(object sender, EventArgs args)
{
    Console.WriteLine($"[SUBSCRIBER LOG]: Order created ({sender})");
}

orderProcessor.OrderCreated += Log;

orderProcessor.Process(orders[0]);

var batchProcessor = new BatchOrderProcessor();
batchProcessor.Process(orders[1]);
