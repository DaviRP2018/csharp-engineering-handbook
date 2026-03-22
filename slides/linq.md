# LINQ

## Why does it exist?

LINQ (Language Integrated Query) exists to solve a historical problem in C# and .NET: the lack of a
consistent, expressive, and type-safe way to query data from different sources.

Before LINQ (pre-C# 3.0, .NET 3.5), developers had several major problems when working with data.

1. Imperative data processing
2. Different query languages for different data sources
3. No compile-time safety
4. Verbose and error-prone code

LINQ was introduced to unify these problems into a single declarative model integrated into the
language

### 1. Imperative data processing

This code says **HOW to iterate**

````csharp
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
````

- Too verbose
- Harder to compose operations
- Easy to break
- Logic spread across loops and conditions

The same result can be achieved with LINQ

This code says **WHAT to iterate**

````csharp
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
````

### 2. Different query languages for different data sources

Before LINQ, developers used different paradigms depending on the data source.

| Data Source   | Query Language   |
|---------------|------------------|
| Objects       | loops            |
| SQL databases | SQL              |
| XML           | XPath / XQuery   |
| Collections   | manual iteration |

LINQ introduced a common abstraction, by building two key interfaces:

- IEnumerable\<T>
- IQueryable\<T>

````mermaid
flowchart LR
    A[queryable data source] --> B[standard query operators] --> C[strongly typed results]
````

````mermaid
flowchart LR
    A[Data Source] --> B[LINQ Query]
    B --> C[Expression / Operators]
    C --> D[Execution]
    D --> E[Result]
````

Data sources can be:

- collections
- databases
- XML
- APIs
- streams

This enabled an important ecosystem of LINQ providers:

| Provider         | Data Source           |
|------------------|-----------------------|
| LINQ to Objects  | in-memory collections |
| LINQ to SQL      | relational databases  |
| Entity Framework | ORM                   |
| LINQ to XML      | XML documents         |

The same syntax works across all of them
Example:

````csharp
users.Where(u => u.IsActive)
````

The execution differs depending on the provider.

### 3. No compile-time safety

Example of SQL before LINQ:

````csharp
var query = "SELECT * FROM Users WHERE Age > 18";
````

Problems:

- No compile-time validation
- Easy runtime errors
- Refactoring unsafe

Meanwhile, LINQ queries are checked by the compiler:

````csharp
users.Where(u => u.Age > 18)
````

### 4. Verbose and error-prone code

Another goal to solve was composability.
LINQ queries behave like pieplines:

````mermaid
flowchart LR
A[Collection] --> B[Where] --> C[Select] --> D[OrderBy] --> E[Take] --> F[Result]
````

Each operator produces a new sequence

This allows flexible query composition:

````csharp
var query =
    users
        .Where(u => u.IsActive)
        .OrderBy(u => u.Name)
        .Take(10)
        .Select(u => u.Email);
````

Also, LINQ introduced the concept of deferred execution. Queries aren't executed immediatly:

````csharp
var query = users.Where(u => u.Age > 18);
````

Execution can be achieved by enumerating the query, for example:
````csharp
ToList()
foreach
First()
Count()
````

