# Resource Management: IDisposable and IAsyncDisposable

In the .NET ecosystem, the Garbage Collector (GC) is responsible for managing memory by
automatically reclaiming space used by objects that are no longer reachable. However, the GC only
manages **managed memory**. Many applications also interact with **unmanaged resources**, such as
file handles, database connections, network sockets, or GDI+ handles—which the GC does not know how
to release efficiently or promptly.

Properly managing these resources is critical to building stable, high-performance applications.
Failing to do so can lead to resource leaks, where the system runs out of available handles or
connections even if there is plenty of RAM available.

### The Problem: Managed vs. Unmanaged Resources

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

### IDisposable: A Contract for Deterministic Cleanup

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

### The 'using' Statement and Declaration

To ensure `Dispose()` is called even if an exception occurs, C# provides the `using` statement.
This is the most common and safest way to work with disposable objects.

#### The 'using' Statement (Classic)

The classic statement defines a scope. When the closing brace is reached, `Dispose()` is
automatically called.

```csharp
using (var stream = new FileStream("data.txt", FileMode.Open))
{
    // Work with the stream
} // Dispose() is called here automatically
```

#### The 'using' Declaration (C# 8.0+)

A more concise syntax where the object is disposed of at the end of the current scope (usually the
end of the method).

```csharp
void ProcessFile()
{
    using var stream = new FileStream("data.txt", FileMode.Open);
    // Work with the stream
    
} // Dispose() is called here when the method returns
```

**Analogy: The Library Book**
Think of a managed object like a book you bought; you can throw it in a corner when done, and
eventually, your "cleaner" (the GC) will pick it up. A disposable object is like a
**library book**. You have a responsibility to return it (Dispose) so others can use it.
If you keep too many library books without returning them, the library (the OS) will eventually
refuse to lend you any more.

### The Standard Dispose Pattern

When creating a class that implements `IDisposable`, especially if it's meant to be inherited, we
follow the **Standard Dispose Pattern**. This ensures that:

1. Resources are disposed of only once.
2. If a developer forgets to call `Dispose()`, the **Finalizer** (Destructor) can act as a
   fallback.

```csharp
public class ResourceWrapper : IDisposable
{
    private bool _disposed = false;

    // The public Dispose method
    public void Dispose()
    {
        Dispose(true);
        // Tell the GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }

    // The protected virtual method for subclasses to override
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Clean up MANAGED resources (other IDisposable objects)
        }

        // Clean up UNMANAGED resources (IntPtr handles, etc.)

        _disposed = true;
    }

    // Finalizer (Destructor) - Only needed if you have raw unmanaged resources
    ~ResourceWrapper()
    {
        Dispose(false);
    }
}
```

### IAsyncDisposable: Handling Asynchronous Cleanup

Modern .NET introduced `IAsyncDisposable` to handle scenarios where closing a resource requires an
asynchronous operation (e.g., flushing a buffer to a remote server or closing a network stream).

```csharp
public interface IAsyncDisposable
{
    ValueTask DisposeAsync();
}
```

If a class implements `IAsyncDisposable`, you should use `await using`:

```csharp
await using (var service = new AsyncService())
{
    await service.DoWorkAsync();
} // DisposeAsync() is awaited here
```

This prevents the calling thread from blocking while waiting for the resource to shut down,
maintaining the benefits of asynchronous programming throughout the entire lifecycle of the object.

### Best Practices and Common Pitfalls

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
