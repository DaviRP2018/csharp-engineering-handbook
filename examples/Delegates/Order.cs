namespace Delegates;

public class Order
{
    public Order()
    {
        OrderNumber = Guid.NewGuid();
    }

    public Guid OrderNumber { get; set; }
    public ShippingProvider ShippingProvider { get; set; }
    public int Total { get; }
    public bool IsReadyForShipment { get; set; } = true;
    public IEnumerable<Item> LineItems { get; set; }
}

public class ProcessedOrder : Order
{
}

public class Item
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }
}
