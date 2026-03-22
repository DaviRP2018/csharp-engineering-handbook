using System.Collections;
using System.Globalization;


#region LINQ Purpose

var users = new List<User>
{
    new([new Order { CreatedAt = new DateTime(2026, 1, 1), Amount = 100m }])
        { Name = "John", Age = 25, IsActive = true },
    new([new Order { CreatedAt = new DateTime(2026, 1, 1), Amount = 1000m }])
        { Name = "Maria", Age = 21, IsActive = true },
};

void NoLinq()
{
    var result = new List<UserSummary>();

    // Iterate over users
    foreach (var user in users)
    {
        // 1. Check if active
        if (!user.IsActive)
            continue;

        // 2. Check age
        if (user.Age < 21)
            continue;

        // 3. declare variables
        decimal totalSpent = 0;
        int recentOrders = 0;

        // 4. iterate over user.Orders
        foreach (var order in user.Orders!)
        {
            // 5. Filter orders from the past 6 months
            if (order.CreatedAt >= DateTime.UtcNow.AddMonths(-6))
            {
                // 6. Aggregate data
                totalSpent += order.Amount;
                recentOrders++;
            }
        }

        // 7. Check if there are recent orders
        if (recentOrders == 0)
            continue;

        // 8. Generate a report
        var summary = new UserSummary
        {
            Name = user.Name,
            Age = user.Age,
            TotalSpent = totalSpent,
            RecentOrders = recentOrders
        };

        // 9. Add to list
        result.Add(summary);
    }

    // 10. Sorting
    result.Sort((a, b) => b.TotalSpent.CompareTo(a.TotalSpent));

    var topUsers = new List<UserSummary>();
    if (topUsers == null) throw new ArgumentNullException(nameof(topUsers));

    // 11. Take the top 10
    for (int i = 0; i < result.Count && i < 10; i++)
    {
        topUsers.Add(result[i]);
    }

    topUsers.ForEach(Console.WriteLine);
}

void WithLinq()
{
    var topUsers = users
            // 1. Check active and age
        .Where(u => u is { IsActive: true, Age: >= 21 })
            // 2. Create an anonymous object with orders from the past 6 months
        .Select(u => new
        {
            u.Name,
            u.Age,
            Orders = u.Orders!
                .Where(o => o.CreatedAt >= DateTime.UtcNow.AddMonths(-6))
        })
            // 3. Check if there are recent orders
        .Where(x => x.Orders.Any())
            // 4. Create an anonymous object with the report, along with aggregations
        .Select(x => new UserSummary
        {
            Name = x.Name,
            Age = x.Age,
            RecentOrders = x.Orders.Count(),
            TotalSpent = x.Orders.Sum(o => o.Amount)
        })
            // 5. Sort
        .OrderByDescending(x => x.TotalSpent)
            // 6. Take the top 10
        .Take(10)
        .ToList();
    
    topUsers.ForEach(Console.WriteLine);
}

#endregion


public record UserSummary
{
    public required string Name;
    public int Age;
    public decimal TotalSpent;
    public int RecentOrders;
}


public class User(IReadOnlyList<Order>? orders)
{
    public required string Name { get; init; }
    public bool IsActive { get; init; }
    public int Age { get; init; }
    public IReadOnlyList<Order>? Orders { get; } = orders;
}

public record Order
{
    public DateTime CreatedAt { get; init; }
    public decimal Amount { get; init; }
}
