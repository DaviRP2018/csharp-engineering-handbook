# Resource Management: IDisposable and IAsyncDisposable

In the .NET ecosystem, the Garbage Collector (GC) is responsible for managing memory by
automatically reclaiming space used by objects that are no longer reachable. However, the GC only
manages **managed memory**. Many applications also interact with **unmanaged resources**, such as
file handles, database connections, network sockets, or GDI+ handles—which the GC does not know how
to release efficiently or promptly.

Properly managing these resources is critical to building stable, high-performance applications.
Failing to do so can lead to resource leaks, where the system runs out of available handles or
connections even if there is plenty of RAM available.

## The Problem: Managed vs. Unmanaged Resources

To understand why we need `IDisposable`, we must distinguish between two types of resources:

1. **Managed Resources**: These are objects created on the managed heap (e.g., `string`, `List<T>`,
   custom classes). The GC tracks these and cleans them up automatically when they are no longer in
   use.
2. **Unmanaged Resources**: These are resources provided by the Operating System or external
   libraries that exist outside the .NET runtime's direct control. Examples include:
    * **Files**: A file lock held by the OS.
    * **Databases**: An active connection to a SQL server.
    * **Network**: An open TCP/IP socket.
    * **Memory**: Memory allocated via `Marshal.AllocHGlobal`.

The GC will eventually reclaim the memory used by the *wrapper* object (e.g., a `FileStream`
instance), but it won't necessarily release the underlying OS handle immediately. This "eventual"
cleanup is non-deterministic, meaning you don't know exactly when it will happen. For a database
connection, waiting several minutes for the GC to run could mean hitting the connection pool limit
and crashing the app.

Let's check the app to see the difference between managed and unmanaged memory

## IDisposable: A Contract for Deterministic Cleanup

The `IDisposable` interface is the standard way in .NET to provide **deterministic cleanup**. It
contains a single method:

```csharp
public interface IDisposable
{
    void Dispose();
}
```

By implementing `IDisposable`, a class signals to its consumers: *"I am holding onto something
important that needs to be released as soon as you are done with me."*

When you call `Dispose()`, you are manually triggering the cleanup of these resources rather than
waiting for the Garbage Collector.

### What Does It Mean Deterministic and Nondeterministic?

Think of it like this:

- Deterministic → "I know exactly what will happen and when"
- Nondeterministic → "I know what can happen, but not exactly when or in what order"

## The 'using' Statement and Declaration

To ensure `Dispose()` is called even if an exception occurs, C# provides the `using` statement.
This is the most common and safest way to work with disposable objects.

### The 'using' Statement (Classic)

The classic statement defines a scope. When the closing brace is reached, `Dispose()` is
automatically called.

```csharp
using (var stream = new FileStream("data.txt", FileMode.Open))
{
    // Work with the stream
} // Dispose() is called here automatically
```

### The 'using' Declaration (C# 8.0+)

A more concise syntax where the object is disposed of at the end of the current scope (usually the
end of the method).

```csharp
void ProcessFile()
{
    using var stream = new FileStream("data.txt", FileMode.Open);
    // Work with the stream
    
} // Dispose() is called here when the method returns
```

**Analogy: The restaurant**
A customer in a restaurant is eating a dish and then leaves the table to go to the
bathroom, leaving his partially eaten food on the table.
The waiter won't (or at least shouldn't right?) clear and clean the customer's table because he
knows that food still "has a reference," the customer in the bathroom.
Until that customer is inside the restaurant, that food has a reference, let's say. But when the
customer is done, pays the bill, and leaves the restaurant, that food is free to be "garbage
collected."

Let's check the WPF form to see the benefit of using IDisposable interface.

## How To Properly Implement The IDisposable Interface – The Standard Dispose Pattern

- https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose

All non-sealed classes should be considered a **potential base class**, because they could be
inherited. If you implement the dispose pattern for any potential base class, you must add the
following methods to your class:

- A Dispose implementation that calls the `Dispose(bool)` method.
- A `Dispose(bool)` method that performs the actual cleanup.
- If your class deals with unmanaged resources, either provide an override to the Object.Finalize
  method or wrap the unmanaged resource in a SafeHandle.

### When Your Class is Sealed

Should at least implement `Dispose()` method:

````csharp
using System;

public sealed class SealedResourceWrapper : IDisposable
{
    // Flag to track whether Dispose has already been called
    // Still needed for idempotency (safe multiple calls)
    private bool _disposed = false;
    
    // The public Dispose method - this is what consumers call
    public void Dispose()
    {
        // Guard clause: if already disposed, do nothing
        // Makes Dispose() idempotent (safe to call multiple times)
        if (_disposed) return;

        // Clean up MANAGED resources (other IDisposable objects)
        // Since this is deterministic cleanup, it's safe to access all managed objects
        // Example:
        // _fileStream?.Dispose();
        // _databaseConnection?.Dispose();

        // Clean up UNMANAGED resources (IntPtr handles, etc.)
        // Example:
        // if (_unmanagedHandle != IntPtr.Zero) {
        //     CloseHandle(_unmanagedHandle);
        //     _unmanagedHandle = IntPtr.Zero;
        // }

        // Mark as disposed to prevent future operations
        _disposed = true;

        // Tell the GC not to call the finalizer since we've already cleaned up
        // This is a performance optimization
        GC.SuppressFinalize(this);
    }
}
````

Real world example:

````csharp
public sealed class SimpleSealedWrapper : IDisposable
{
    private readonly FileStream _fileStream;
    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    public SimpleSealedWrapper()
    {
        _fileStream = new FileStream("data.txt", FileMode.OpenOrCreate);
        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Clean up managed disposable resources
        _fileStream?.Dispose();
        _httpClient?.Dispose();

        _disposed = true;
        
        // Still call GC.SuppressFinalize even without a finalizer
        // This is harmless and maintains consistency
        GC.SuppressFinalize(this);
    }

    // No finalizer needed since we only have managed resources
    // The GC will eventually clean up the managed objects
}
````

### When the class is Non-Sealed

Any non-sealed class should have an `Dispose(bool)` overload method because the class could
potentially be inherited.

In the overload, the `disposing` parameter is a `Boolean` that indicates whether the method call
comes from a `Dispose` method (its value is `true`) or from a finalizer (its value is `false`).

````csharp
public class NonSealedResourceWrapper : IDisposable
{
    // Flag to track whether Dispose has already been called
    // This prevents duplicate cleanup and allows idempotent Dispose calls
    private bool _disposed = false;

    // The public Dispose method - this is what consumers call
    // This method implements the deterministic cleanup contract of IDisposable
    public void Dispose()
    {
        // Call the protected Dispose method with true to indicate
        // this is a deliberate cleanup (not from finalizer)
        Dispose(true);
        
        // Tell the GC not to call the finalizer since we've already cleaned up
        // This is a performance optimization - avoids unnecessary finalization queue processing
        // and prevents the object from living longer than needed (finalizable objects survive
        // an extra GC cycle)
        GC.SuppressFinalize(this);
    }

    // The protected virtual method for subclasses to override
    // This is the core of the dispose pattern - it handles both deterministic
    // and non-deterministic cleanup scenarios
    protected virtual void Dispose(bool disposing)
    {
        // Guard clause: if already disposed, do nothing
        // This makes Dispose() idempotent (safe to call multiple times)
        if (_disposed) return;

        // The 'disposing' parameter tells us HOW we got here:
        // - true: Called from Dispose() method (deterministic cleanup)
        // - false: Called from finalizer (non-deterministic cleanup)
        if (disposing)
        {
            // Clean up MANAGED resources (other IDisposable objects)
            // We only do this when disposing=true because:
            // 1. During finalization, other managed objects may already be finalized
            // 2. The GC will handle managed memory anyway
            // 3. Accessing managed objects from finalizer thread can be dangerous
            
            // Example: if we had fields like:
            // _fileStream?.Dispose();
            // _databaseConnection?.Dispose();
        }

        // Clean up UNMANAGED resources (IntPtr handles, Win32 handles, etc.)
        // This happens regardless of how we got here because:
        // 1. Unmanaged resources won't be cleaned up automatically
        // 2. We must release them whether called from Dispose() or finalizer
        // 3. This is our last chance to prevent resource leaks
        
        // Example: if we had unmanaged resources:
        // if (_unmanagedHandle != IntPtr.Zero) {
        //     CloseHandle(_unmanagedHandle);
        //     _unmanagedHandle = IntPtr.Zero;
        // }

        // Mark as disposed to prevent future operations and duplicate cleanup
        _disposed = true;
    }

    // Finalizer (Destructor) - Only needed if you have direct unmanaged resources
    // This provides a safety net for non-deterministic cleanup when consumers
    // forget to call Dispose() explicitly
    ~NonSealedResourceWrapper()
    {
        // Call Dispose with false to indicate this is finalization, not explicit disposal
        // This means:
        // - Don't touch managed resources (they may already be finalized)
        // - Only clean up unmanaged resources
        // - This is our last chance to prevent resource leaks
        Dispose(false);
    }
}
````

Here's How a Derived Class Would Extend It:

````csharp
public class DerivedResourceWrapper : NonSealedResourceWrapper
{
    private FileStream _additionalResource;
    private bool _derivedDisposed = false;

    // The derived class overrides the protected virtual method
    protected override void Dispose(bool disposing)
    {
        // Guard clause for this level
        if (_derivedDisposed) return;

        if (disposing)
        {
            // Clean up managed resources specific to derived class
            _additionalResource?.Dispose();
        }

        // Clean up unmanaged resources specific to derived class
        // (if any)

        _derivedDisposed = true;

        // CRITICAL: Call base class disposal
        // This ensures the entire inheritance chain gets cleaned up
        base.Dispose(disposing);
    }
}
````

> **Important!**
>
> A finalizer (a `Object.Finalize` override) is only required if you directly reference unmanaged
> resources. **This is a highly advanced scenario that can be typically avoided**:
> - If your class references only managed objects, it's still possible for the class to implement
    the dispose pattern. There's no need to implement a finalizer.
> - If you need to deal with unmanaged resources, we strongly recommend wrapping the unmanaged
    `IntPtr` handle into a `SafeHandle`. The `SafeHandle` provides a finalizer so you don't have to
    write one yourself.
>
> Again, it's recommended to avoid implementing a finalizer.

### What's a Finalizer?

In C#, a finalizer looks like a destructor, but it's different from in C++.
You declare it like this, using the tilt (~) symbol.

````csharp
class MyClass
{
    ~MyClass()
    {
        // cleanup code
    }
}
````

When should you use a finalizer?
Almost never directly.

Observation:

- You cannot call it manually
- You cannot control when it runs
- It is executed by the GC on a separate thread

A finalizer is part of nondeterministic cleanup:

- it runs at an unknown time
- it may run much later
- it might not run at all before process exit

### Cascade dispose calls

If your class owns an instance of another type that implements `IDisposable`, the containing class
itself should also implement `IDisposable`. Typically a class that instantiates an `IDisposable`
implementation and stores it as an instance member (or property) is also **responsible for its
cleanup**. This helps ensure that the referenced disposable types are given the opportunity to
deterministically perform cleanup through the `Dispose` method. In the following example, the class
is sealed.

````csharp
using System;

public sealed class Foo : IDisposable
{
    private readonly IDisposable _bar;

    public Foo()
    {
        _bar = new Bar();
    }

    public void Dispose() => _bar.Dispose();
}
````

If your class has an `IDisposable` field or property but doesn't _own_ it, then the class doesn't
need to implement `IDisposable`. Typically a class creating and storing the `IDisposable` child
object also becomes the owner, but in some cases the ownership can be transferred to another
`IDisposable` type.

#### Ownership is not a language feature, it's a design responsibility.

- If your class owns an object → it must dispose it
- If it does not own it → it must NOT dispose it

How to identify ownership
Think in terms of who created it and who controls its lifetime.

If your class instantiates the dependency, it owns it.

**Case 1: You create it → You own it**

````csharp
class MyService : IDisposable
{
    private readonly StreamReader _reader = new StreamReader("file.txt");

    public void Dispose()
    {
        _reader.Dispose(); // you own it → you dispose it
    }
}
````

**Case 2: It is injected → You probably don't own it**

Here, the object comes from outside.

- You didn't create it
- Someone else might be using it → You don't own it

````csharp
class MyService
{
    private readonly StreamReader _reader;

    public MyService(StreamReader reader)
    {
        _reader = reader;
    }
}
````

## IAsyncDisposable: Handling Asynchronous Cleanup

The `System.IAsyncDisposable` interface was introduced as part of C# 8.0. You implement the
`IAsyncDisposable.DisposeAsync()` method when you need to perform resource cleanup, just as you
would when implementing a Dispose method. One of the key differences, however, is that this
implementation allows for asynchronous cleanup operations. The `DisposeAsync()` returns a
`ValueTask` that represents the asynchronous disposal operation (e.g., flushing a buffer to a
remote server or closing a network stream).

It's typical when implementing the `IAsyncDisposable` interface that classes **also implement the
`IDisposable` interface**. A good implementation pattern of the `IAsyncDisposable` interface is to
be prepared for either synchronous or asynchronous disposal, **however, it's not a requirement**.
If no synchronous disposable of your class is possible, having only `IAsyncDisposable` is
acceptable. All the guidance for implementing the disposal pattern also applies to the
asynchronous implementation.

> **Note:**
>
> If you implement the `IAsyncDisposable` interface but not the `IDisposable` interface,
> your app can potentially leak resources. If a class implements `IAsyncDisposable`, but not
`IDisposable`, and a consumer only calls `Dispose`, your implementation would never call
`DisposeAsync`. This would result in a resource leak.

The IAsyncDisposable interface declares:

- A `public IAsyncDisposable.DisposeAsync()` implementation that has no parameters.
- Any nonsealed class should define a `protected virtual ValueTask DisposeAsyncCore()` method whose
  signature is:

````csharp
protected virtual ValueTask DisposeAsyncCore()
{
}
````

> **What is a ValueTask?**
>
> A `ValueTask` is a structure that can wrap either a `Task` or a `IValueTaskSource` instance.
> The following operations should never be performed on a `ValueTask` instance:
>
> - Awaiting the instance multiple times.
> - Calling `AsTask` multiple times.
> - Using more than one of these techniques to consume the instance.
>
> For more information,
> see: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask?view=net-10.0

## How To Properly Implement The IAsyncDisposable Interface – The Standard Dispose Pattern

- https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync

Here are the examples:

### 1. When Your Class is Sealed and Fully Async

Can implement only `DisposeAsync`. `DisposeAsyncCore` is desirable but not needed.

````csharp
public sealed class MyServiceAsync : IAsyncDisposable
{
    private bool _disposed = false;
    private IAsyncDisposable? _asyncResource = new MyAsyncResource();

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_asyncResource is not null)
        {
            await _asyncResource.DisposeAsync().ConfigureAwait(false);
        }

        _asyncResource = null;
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }
}
````

### 2. When Your Class is Non-Sealed and Fully Async

Needs to implement at least both `DisposeAsync` and `DisposeAsyncCore`

The `DisposeAsyncCore()` method is intended to perform the asynchronous cleanup of managed
resources or for cascading calls to `DisposeAsync()`. It encapsulates the common asynchronous
cleanup operations when a subclass inherits a base class that is an implementation of
`IAsyncDisposable.` The `DisposeAsyncCore()` method is virtual so that derived classes can define
custom cleanup in their overrides.

````csharp
public class MyServiceAsync : IAsyncDisposable
{
    private bool _disposed = false;
    private IAsyncDisposable? _asyncResource = new MyAsyncResource();

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;

        if (_asyncResource is not null)
        {
            await _asyncResource.DisposeAsync().ConfigureAwait(false);
        }

        _asyncResource = null;
        _disposed = true;
    }
}
````

### 3. When Your Class is Sealed and Needs Synchronous Disposal

Can implement both `Dispose` and `DisposeAsync` only.

````csharp
public sealed class MyServiceAsync : IDisposable, IAsyncDisposable
{
    private bool _disposed = false;
    private bool _asyncDisposed = false;
    IDisposable? _disposableResource = new MemoryStream();
    IAsyncDisposable? _asyncDisposableResource = new MemoryStream();

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposableResource?.Dispose();
        _disposableResource = null;

        if (_asyncDisposableResource is IDisposable disposable)
        {
            disposable.Dispose();
            _asyncDisposableResource = null;
        }
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_asyncDisposed) return;
        
        if (_asyncDisposableResource is not null)
        {
            await _asyncDisposableResource.DisposeAsync().ConfigureAwait(false);
        }

        if (_disposableResource is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _disposableResource?.Dispose();
        }

        _asyncDisposableResource = null;
        _disposableResource = null;
        
        _asyncDisposed = true;
        GC.SuppressFinalize(this);
    }
}
````

### 4. When Your Class is Non-Sealed and Needs Synchronous Disposal

Needs full implementation: `Dispose`, `Dispose(bool)`, `DisposeAsync` and `DisposeAsyncCore`.

- The `ExampleAsyncDisposable` is a nonsealed class that implements the `IAsyncDisposable`
  interface.
- It contains a private `IAsyncDisposable` field, `_example`, that's initialized in the
  constructor.
- The `DisposeAsync` method delegates to the `DisposeAsyncCore` method and calls
  `GC.SuppressFinalize` to notify the garbage collector that the finalizer doesn't have to run.
- It contains a `DisposeAsyncCore()` method that calls the `_example.DisposeAsync()` method, and
  sets the field to null.
- The `DisposeAsyncCore()` method is `virtual`, which allows subclasses to override it with custom
  behavior.

````csharp
class MyServiceAsync : IDisposable, IAsyncDisposable
{
    private bool _disposed = false;
    private bool _asyncDisposed = false;
    IDisposable? _disposableResource = new MemoryStream();
    IAsyncDisposable? _asyncDisposableResource = new MemoryStream();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            _disposableResource?.Dispose();
            _disposableResource = null;

            if (_asyncDisposableResource is IDisposable disposable)
            {
                disposable.Dispose();
                _asyncDisposableResource = null;
            }
        }
        
        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_asyncDisposed) return;
        
        if (_asyncDisposableResource is not null)
        {
            await _asyncDisposableResource.DisposeAsync().ConfigureAwait(false);
        }

        if (_disposableResource is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _disposableResource?.Dispose();
        }

        _asyncDisposableResource = null;
        _disposableResource = null;
        _asyncDisposed = true;
    }
}
````

> **Note**
>
> One primary difference in the async dispose pattern compared to the dispose pattern, is that the
> call from `DisposeAsync()` to the `Dispose(bool)` overload method is given `false` as an
> argument. When implementing the `IDisposable.Dispose()` method, however, `true` is passed
> instead. This helps ensure functional equivalence with the synchronous dispose pattern, and
> further ensures that finalizer code paths still get invoked. In other words, the
`DisposeAsyncCore()` method will dispose of managed resources asynchronously, so you don't want to
> dispose of them synchronously as well. Therefore, call `Dispose(false)` instead of
`Dispose(true)`.

> **Note:** `static` classes are `sealed`

## What About a Finalizer in Async Pattern?

There is NO finalizer in async programming. A finalizer will always be synchronous and should be
avoided.

- There is NO async finalizer
- Finalizer ≠ async cleanup
- Finalizer = last resource for unmanaged

## Using async disposable

To properly consume an object that implements the `IAsyncDisposable` interface, you use the `await`
and `using` keywords together. Consider the following example, where the `ExampleAsyncDisposable`
class is instantiated and then wrapped in an `await using` statement.

````csharp
class ExampleUsingStatementProgram
{
    static async Task Main()
    {
        await using (var exampleAsyncDisposable = new ExampleAsyncDisposable())
        {
            // Interact with the exampleAsyncDisposable instance.
        }

        Console.ReadLine();
    }
}
````

For situations where the usage of `ConfigureAwait` is needed, the `await using` statement could be
as follows:

````csharp
class ExampleConfigureAwaitProgram
{
    static async Task Main()
    {
        var exampleAsyncDisposable = new ExampleAsyncDisposable();
        await using (exampleAsyncDisposable.ConfigureAwait(false))
        {
            // Interact with the exampleAsyncDisposable instance.
        }

        Console.ReadLine();
    }
}
````

### ConfigureAwait Extension Method

Use the `ConfigureAwait(IAsyncDisposable, Boolean)` extension method of the `IAsyncDisposable`
interface to configure how the continuation of the task is marshaled on its original context or
scheduler.

````csharp
class ExampleConfigureAwaitProgram
{
    static async Task Main()
    {
        var exampleAsyncDisposable = new ExampleAsyncDisposable();
        await using (exampleAsyncDisposable.ConfigureAwait(false))
        {
            // Interact with the exampleAsyncDisposable instance.
        }

        Console.ReadLine();
    }
}
````

#### What is ConfigureAwait(bool)?

- https://devblogs.microsoft.com/dotnet/configureawait-faq/

When you do:

````csharp
await SomeAsync();
````

By default, it's doing this:

````csharp
await SomeAsync().ConfigureAwait(true);
````

When it's `true` you are telling to capture the actual context.

**Behavior**

| Option                  | Description                                                              |
|-------------------------|--------------------------------------------------------------------------|
| `ConfigureAwait(true)`  | Captures the current context and resumes on it after the await           |
| `ConfigureAwait(false)` | Does not capture the context; continuation may run on a different thread |

**Why would I want to use ConfigureAwait(false)?**

`ConfigureAwait(continueOnCapturedContext: false)` is used to avoid forcing the callback to be
invoked on the original context or scheduler. This has a few benefits:

- Improving performance: If the code after an `await` doesn’t actualy require running in the
  original context, using `ConfigureAwait(false)` can avoid all these costs: it won’t need to queue
  unnecessarily, it can use all the optimizations it can muster, and it can avoid the
  unnecessary thread static accesses.
- Avoiding deadlocks: Can cause deadlocks in specific scenarios. Let's see this in the following
  example.

Imagine a WPF form with the following code:

````csharp
1.  public static void Main()
2.  {
3.      // Simulating a UI thread blocking
4.      var result = GetDataAsync().Result; // Blocks the thread
5.      Console.WriteLine(result);
6.  }
7.  
8.  public static async Task<string> GetDataAsync()
9.  {
10.     await Task.Delay(1000); // Captures context by default
11.     Console.WriteLine("I won't be reached")
12.     return "Done";
13. }
````

So, have in mind this:

- In WPF there is a context, which is the UI thread
- UI thread will reach Line 4 and call the async method
- The UI thread enters the method and in line 10 will delegate the Task to another thread to
  execute it, and then the UI Thread will go back to line 4 to wait for that thread to continue the
  method and return the task completed
- Since in line 4 we have `.Result`, the **UI thread** will wait at line 4, in blocked status
- When the task is completed, since we asked `ConfigureAwait(true)`, the thread MUST return the
  completed task to the thread of the current context, which is the **UI thread**
- But the UI thread is with status **blocked**, and can't accept any tasks because it's waiting for
  itself!

In a Console App, there is no SynchronizationContext, so continuations run on the ThreadPool,
instead of a single UI thread, avoiding this issue.

Let's check the WPF "ConfigureAwaitDeadlock" to see that deadlock.

## Stacked usings

In situations where you create and use multiple objects that implement `IAsyncDisposable`, it's
possible that stacking `await using` statements with `ConfigureAwait` could prevent calls to
`DisposeAsync()` in errant conditions. To ensure that `DisposeAsync()` is always called, you should
avoid stacking. The following three code examples show acceptable patterns to use instead.

Basically, in all acceptable patterns you can see that the disposal happens sequentially. The
constructor of the classes being initialized must at least complete successfully so it's possible
to call their disposal methods.

### Acceptable pattern one

````csharp
class ExampleOneProgram
{
    static async Task Main()
    {
        var objOne = new ExampleAsyncDisposable();
        await using (objOne.ConfigureAwait(false))
        {
            // Interact with the objOne instance.

            var objTwo = new ExampleAsyncDisposable();
            await using (objTwo.ConfigureAwait(false))
            {
                // Interact with the objOne and/or objTwo instance(s).
            }
        }

        Console.ReadLine();
    }
}
````

### Acceptable pattern two

````csharp
class ExampleTwoProgram
{
    static async Task Main()
    {
        var objOne = new ExampleAsyncDisposable();
        await using (objOne.ConfigureAwait(false))
        {
            // Interact with the objOne instance.
        }

        var objTwo = new ExampleAsyncDisposable();
        await using (objTwo.ConfigureAwait(false))
        {
            // Interact with the objTwo instance.
        }

        Console.ReadLine();
    }
}
````

### Acceptable pattern three

````csharp
class ExampleThreeProgram
{
    static async Task Main()
    {
        var objOne = new ExampleAsyncDisposable();
        await using var ignored1 = objOne.ConfigureAwait(false);

        var objTwo = new ExampleAsyncDisposable();
        await using var ignored2 = objTwo.ConfigureAwait(false);

        // Interact with objOne and/or objTwo instance(s).

        Console.ReadLine();
    }
}
````

### Unacceptable pattern

````csharp
class DoNotDoThisProgram
{
    static async Task Main()
    {
        var objOne = new ExampleAsyncDisposable();
        // Exception thrown on .ctor
        var objTwo = new AnotherAsyncDisposable();

        await using (objOne.ConfigureAwait(false))  // Bad
        await using (objTwo.ConfigureAwait(false))  // Bad
        {
            // Neither object has its DisposeAsync called.
        }

        Console.ReadLine();
    }
}
````

## Best Practices and Common Pitfalls

1. **Dispose is NOT for memory management**: Do not implement `IDisposable` just to null out large
   lists or strings. Let the GC do its job for managed memory.
2. **Idempotency**: `Dispose()` should be safe to call multiple times. Subsequent calls should do
   nothing.
3. **Encapsulation**: If your class owns an `IDisposable` field, your class should usually
   implement `IDisposable` as well to propagate the cleanup.
4. **Exceptions in Dispose**: Avoid throwing exceptions from within `Dispose()`. It can interfere
   with the cleanup process and hide original exceptions that occurred during the `using` block.
5. **Accessing Disposed Objects**: Once `Dispose()` is called, the object is technically "dead."
   It's good practice to throw an `ObjectDisposedException` if any other methods are called on it
   afterward.
