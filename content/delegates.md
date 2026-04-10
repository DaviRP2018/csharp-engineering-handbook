# Delegates

## Why Does It Exist?

Delegates in C# exist to enable passing **behavior as parameters** while preserving type safety and
compile-time validation.

The core problem delegates solve is **rigid and tightly coupled code**. Instead of a method having
to know every possible way to perform a task, it can receive the "how" as a parameter.

## The Problem It Solves

Before delegates (or without them), extending behavior often required:

- **Hardcoding logic** using `if/else` or `switch` statements.
- **Creating deep inheritance hierarchies** just to change one small piece of logic.
- **Code duplication** for slight variations in processing.

This violates the **Open/Closed Principle** (code should be open for extension but closed for
modification).

### Without Delegates (The Hard Way)

If you want to support different ways of processing an order, you might end up with code like this:

````csharp
void ProcessAll(IEnumerable<Order> orders, string type) 
{
    foreach(var order in orders) 
    {
        if (type == "standard") { /* ... logic ... */ }
        else if (type == "express") { /* ... logic ... */ }
        else if (type == "international") { /* ... logic ... */ }
    }
}
````

Every time you add a new shipping method, you must modify this method, increasing the risk of bugs.

### With Delegates (The Extensible Way)

With delegates, the method doesn't care about the "type" anymore. It just executes the behavior
provided:

````csharp
void ProcessAll(IEnumerable<Order> orders, Action<Order> processor) 
{
    foreach(var order in orders) 
    {
        processor(order);
    }
}
````

## Creating A Delegate

A delegate is essentially a **type-safe function pointer**. It defines a contract (a method
signature) that any assigned method must fulfill.

### 1. Defining the Delegate

You use the `delegate` keyword followed by the return type and parameters. Think of it as an
"interface for a single method."

````csharp
public delegate Order ProcessOrder(Item item, int quantity);
````

### 2. Matching the Signature

Any method that matches this signature (same return type and parameters) can be assigned to this
delegate.

````csharp
Order AddToCart(Item item, int quantity) { ... }
Order BuyNow(Item item, int quantity) { ... }

// Both are valid assignments
ProcessOrder handler = AddToCart;
handler = BuyNow;
````

> [!TIP]
> Delegates support **Multicast**, meaning a single delegate variable can hold references to
> multiple methods. When invoked, they are called in the order they were added using the `+=`
> operator.

## Covariance & Contravariance


| Type           | What changes   | Safe substitution rule   | Keyword |
|----------------|----------------|--------------------------|---------|
| Covariance     | Return type    | Can return more specific | `out`   |
| Contravariance | Parameter type | Can accept more generic  | `in`    |

The real rule is:
- Outputs can be more specific
- Inputs can be more generic

Real definition:

````csharp
public delegate TResult Func<out TResult>();
public delegate void Action<in T>(T obj);
````

### Examples

Suppose we have these classes:

````csharp
public class Animal {}
public class Dog : Animal {}
public class GoldenRetriever : Dog {}
````

#### Covariance (Func<Dog>)

GoldenRetriever → Dog → Animal (going UP is fine for assignment target)

````csharp
Func<Dog> getAnimal = () => new GoldenRetriever(); // covariance
````

Animal → ❌ Can't return (too generic)
Dog → ✅ Can (exact match)
GoldenRetriever → ✅ Can (more specific, still a Dog)

The delegate promises: “I will return a Dog”
Returning a more specific type (GoldenRetriever) is safe.
Returning a more generic type (Animal) is NOT safe.

#### Contravariance (Action<GoldenRetriever>)

Animal ← Dog ← GoldenRetriever

````csharp
public static void HandleAnimal(Animal a) {}

Action<Dog> act = HandleAnimal;
````

Animal → ✅ Can (more generic, still an Animal)
Dog → ✅ Can (exact match)
GoldenRetriever → ❌ (too specific)

The delegate promises: “I will receive a Dog”
Returning a more generic type (Animal) is safe. Because a DOG is an ANIMAL.
Returning a more specific type (GoldenRetriever) is NOT safe. Because a DOG isn't a Golden
Retriever. Actually, it's the reverse, a Golden Retriever is a Dog.

## More details about delegates

Delegates are public, so anyone can set or add more functions to your delegate.
A delegate can have multiple functions assigned to it, where they will be executed sequentially

Suppose we have this class

````csharp
class OrderProcessor
{
    public delegate bool ProduceShippingLabel(Order order);
    public ProduceShippingLabel? OnProduceShippingLabel { get; set; }

    public void Process(Order order)
    {
        Console.WriteLine("Order being processed.");

        OnProduceShippingLabel.Invoke(order);

        Console.WriteLine("Order finished.");
    }
}
````

Now let's create some methods that match the delegate signature

````csharp
var orderProcessor = new OrderProcessor();

static bool BlueLabelShipment()
{
    Console.WriteLine("I'm labeling this order package!");
    return true;
}

static bool GoldenLabelShipment()
{
    Console.WriteLine("Labeling golden label for order!");
    return true;
}

static bool AddMoreLabels()
{
    Console.WriteLine("Adding more unnecessary labels for order to confuse people!");
    return true;
}
````

And now we can assign the methods to the delegate pointer

- Using `+=`

````csharp
orderProcessor.OnProduceShippingLabel = GoldenLabelShipment;
orderProcessor.OnProduceShippingLabel += AddMoreLabels;
````

- Using a variable to hold the methods

````csharp
var chain = GoldenLabelShipment;
chain += AddMoreLabels;
orderProcessor.OnProduceShippingLabel = chain;
````

- Using `+` operator, but requires to cast the first delegate being added

````csharp
orderProcessor.OnProduceShippingLabel = (orderProcessor.OnProduceShippingLabel)GoldenLabelShipment + AddMoreLabels;
````

We can get info about the methods being pointed to

````csharp
Console.WriteLine(orderProcessor.OnProduceShippingLabel.GetMethodInfo());
// Outputs last added method: Boolean <<Main>$>g__AddMoreLabels|0_2(Delegates.Domain.Order)

foreach (var met in orderProcessor.OnProduceShippingLabel.GetInvocationList())
{
    // Outputs each delegate with more attributes
    Console.WriteLine(met.Method);
}
// Outputs:
// Boolean <<Main>$>g__GoldenLabelShipment|0_1(Delegates.Domain.Order)
// Boolean <<Main>$>g__AddMoreLabels|0_2(Delegates.Domain.Order)
````

You can add multiple equal delegates, in which will run them sequentially

````csharp
orderProcessor.OnProduceShippingLabel = GoldenLabelShipment;
orderProcessor.OnProduceShippingLabel += GoldenLabelShipment;
orderProcessor.OnProduceShippingLabel += GoldenLabelShipment;
````

To remove delegates, use `-=` and the method to be removed

````csharp
orderProcessor.OnProduceShippingLabel -= GoldenLabelShipment;
````

if you try to remove a method that isn't in the pointer, nothing happens and there are no
exceptions

````csharp
orderProcessor.OnProduceShippingLabel -= MyMagicMethod;
````

## Built-in Delegates

C# provides a set of built-in generic delegates that cover 99% of use cases. **You should prefer
these** over custom delegates unless you need a very specific domain name for clarity.

### 1. `Action<...>`

Used for methods that **return void**.

- `Action`: No parameters.
- `Action<T1, T2, ...>`: Up to 16 parameters.

### 2. `Func<..., TResult>`

Used for methods that **return a value**.

- `Func<TResult>`: No parameters, returns `TResult`.
- `Func<T, TResult>`: One parameter, returns `TResult`. The **last** type argument is always the
  return type.

### 3. `Predicate<T>`

Used for methods that return a `bool`.

- Equivalent to `Func<T, bool>`.
- Commonly used in collections (e.g., `List.Find`).

### 4. `EventHandler` & `EventHandler<T>`

The standard pattern for **Events**.

- `(object sender, EventArgs e)`
- Discussed in detail in the [Events](#events) section.

# Lambdas

Lambdas (or anonymous functions) allow you to define a method inline, right where you need it. This
is especially useful when a method expects a delegate and the logic you want to provide is
short-lived or specific to that single call.

A lambda is essentially a shorthand for a method. It consists of three parts:

- **Parameters** (on the left)
- **The Lambda Operator** (`=>`, read as "goes to")
- **The Body** (on the right)

## Syntax Patterns

### 1. Lambda Expression

A concise one-liner where the result is automatically returned. No curly braces or `return` keyword
needed.

````csharp
// (parameters) => expression
Func<int, int, int> add = (a, b) => a + b;
````

### 2. Lambda Statement

Useful for more complex logic. It uses a code block `{}` and requires an explicit `return` if the
delegate expects a result.

````csharp
// (parameters) => { statements; }
Action<string> greet = name => 
{
    string message = $"Hello, {name}!";
    Console.WriteLine(message);
};
````

## Type Inference

The C# compiler is smart enough to infer the types of the parameters and the return type based on
the delegate it's being assigned to.

````csharp
// No need to specify 'int x'. The compiler knows 'x' is an 'int' because of 'Predicate<int>'.
Predicate<int> isPositive = x => x > 0;
````

However, you can explicitly define types if it improves readability or if the compiler needs help:

````csharp
Func<int, string, bool> checker = (int age, string name) => age > 18 && name.Length > 0;
````

## Capturing Variables (Closures)

Lambdas can "capture" variables from the scope where they are defined. This makes them much more
powerful than traditional methods for callbacks.

````csharp
int threshold = 10;
Func<int, bool> isAboveThreshold = x => x > threshold;

Console.WriteLine(isAboveThreshold(15)); // True
threshold = 20;
Console.WriteLine(isAboveThreshold(15)); // False
````

> [!IMPORTANT]
> A captured variable is only disposed when the delegate is no longer used and marked for disposal.
> This can lead to unexpected memory retention if not careful.

## Static Lambdas

If you want to ensure a lambda doesn't accidentally capture any variables from the outer scope (
avoiding side effects or memory overhead), you can declare it as `static`.

````csharp
int constant = 5;
// This would fail to compile if it tried to use 'constant'
Func<int, int> doubleIt = static x => x * 2; 
````

## Removing Lambdas from a Chain

One drawback of using lambdas with multicast delegates (chains) is that they are anonymous. Since
you don't have a named reference to the method, you cannot easily remove it from the chain using
the `-=` operator.

````csharp
processor.ProcessCompleted += (s) => Console.WriteLine(s);
// How do you remove it? You can't easily, because the lambda above is "lost".
````

If you need to remove a delegate later, use a named method instead.
Otherwise, you will need to reset the entire pointer with:

````csharp
processor.ProcessCompleted = null;
````

# Events

Events allow a class (the **Publisher**) to notify other classes (**Subscribers**) when something
interesting happens. While events are built on top of delegates, they provide a layer of
encapsulation that makes your code safer and more robust.

## Why Use Events instead of Public Delegates?

If you expose a delegate as a public field, any part of your application can:

1. **Invoke it:** External code could trigger the "Click" logic of a button.
2. **Clear it:** Someone could accidentally use `=` instead of `+=`, removing all other
   subscribers.

The `event` keyword restricts these actions:

- **Only the Publisher** can invoke the event.
- **Subscribers** can only use `+=` (subscribe) and `-=` (unsubscribe).

## Standard Event Pattern

In .NET, events should follow a standard pattern using the `EventHandler` delegate.

### 1. The Event Data (EventArgs)

To pass custom data, create a class inheriting from `EventArgs`. The naming convention is to end
the class name with `EventArgs`.

````csharp
public class OrderEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderEventArgs(Order order) => Order = order;
}
````

### 2. The Event Declaration

Always use `void` as the return type. The standard `EventHandler` delegate pattern is:
- `object? sender`: The instance that raised the event.
- `TEventArgs e`: Data associated with the event.

Use the generic `EventHandler<TEventArgs>` for custom data, or the non-generic `EventHandler` for
events without data (equivalent to `EventHandler<EventArgs>`).

````csharp
public class OrderProcessor
{
    // Without custom data (uses EventArgs.Empty)
    public event EventHandler? OrderStarted;

    // With custom data
    public event EventHandler<OrderEventArgs>? OrderCompleted;
}
````

### 3. Raising the Event (The "On" Pattern)

The recommended practice is to create a `protected virtual` method to raise the event (e.g.,
`OnOrderCompleted`).

- **Protected:** Prevents external classes from "faking" the event.
- **Virtual:** Allows derived classes to override the behavior (e.g., adding logging or canceling
  the event) before or after calling `base.OnOrderCompleted()`.

````csharp
protected virtual void OnOrderCompleted(Order order)
{
    // Use the null-conditional operator to avoid NullReferenceException
    // and provide thread-safety by capturing the delegate copy.
    OrderCompleted?.Invoke(this, new OrderEventArgs(order));
}
````

> [!TIP]
> Using `?.Invoke(this, ...)` is thread-safe because it captures a temporary copy of the delegate
> before checking for null, preventing a race condition where a subscriber unregisters between the
> check and the call.

### 4. Subscribing to Events

Subscribers "attach" their logic using the `+=` operator.

````csharp
var processor = new OrderProcessor();

// Subscribing with a method
processor.OrderCompleted += HandleOrderCompleted;

// Subscribing with a lambda
processor.OrderCompleted += (sender, args) => 
{
    Console.WriteLine($"Order {args.OrderId} processed!");
};

void HandleOrderCompleted(object? sender, OrderEventArgs e)
{
    // Logic here
}
````

## Summary of Guidelines

- **Naming:** Events should be named using a verb or verb phrase (e.g., `Closing`, `Closed`).
- **Signature:** Always use `(object sender, EventArgs e)`.
- **Encapsulation:** Always use the `event` keyword to prevent external invocation.
- **Data:** If you need to pass data, create a class that inherits from `EventArgs`.

## Full Example: Publisher and Subscriber

Here is a complete example of a **Publisher** (the class that raises the event) and a **Subscriber
** (the class that reacts to it).

````csharp
// 1. Define custom event arguments
public class MessageEventArgs : EventArgs
{
    public string Message { get; }
    public DateTime SentAt { get; }
    public MessageEventArgs(string message) => (Message, SentAt) = (message, DateTime.Now);
}

// 2. THE PUBLISHER: Defines and raises the event
public class MessageBroadcaster
{
    // The event declaration
    public event EventHandler<MessageEventArgs>? MessageReceived;

    // The method that triggers the event logic
    public void Broadcast(string text)
    {
        Console.WriteLine($"[Publisher] Broadcasting: {text}");
        OnMessageReceived(text);
    }

    // Standard 'On' pattern for raising the event
    protected virtual void OnMessageReceived(string text)
    {
        MessageReceived?.Invoke(this, new MessageEventArgs(text));
    }
}

// 3. THE SUBSCRIBER: Attaches behavior to the publisher's event
public class LoggingService
{
    public void Subscribe(MessageBroadcaster broadcaster)
    {
        // Subscribe using the += operator
        broadcaster.MessageReceived += HandleMessage;
    }

    // The event handler method (matches the EventHandler<T> signature)
    private void HandleMessage(object? sender, MessageEventArgs e)
    {
        Console.WriteLine($"[Subscriber] Logged at {e.SentAt}: {e.Message}");
    }
}

// --- Usage ---
var broadcaster = new MessageBroadcaster(); // The Publisher
var logger = new LoggingService();           // The Subscriber

logger.Subscribe(broadcaster);               // Attach the relationship
broadcaster.Broadcast("Hello, Events!");     // Raise the event
````

In this example:

- **`MessageBroadcaster`** is the **Publisher**. It owns the `MessageReceived` event and decides
  when to trigger it.
- **`LoggingService`** is the **Subscriber**. It doesn't know *how* or *when* the broadcast
  happens; it only cares about reacting to the event when it is raised.
