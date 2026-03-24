# Asynchronous Programming in C#

Asynchronous programming is a fundamental pillar of modern software development, particularly in
the .NET ecosystem. It allows applications to remain responsive while performing long-running or
I/O-bound operations, ensuring efficient resource utilization without blocking the execution flow.

### What is a Thread and How Async Treats It

A **Thread** is the smallest unit of execution within an operating system. Creating and managing
threads is expensive in terms of memory and CPU overhead.

In traditional synchronous programming, a thread is tied to a single task until it finishes. In
asynchronous programming, the relationship is decoupled. When an `await` is encountered on a
non-completed task, the current thread is released back to the **Thread Pool**. Once the awaited
task completes, a thread (possibly the same one, or a different one depending on the context)
resumes the execution from where it left off. This mechanism prevents "Thread Starvation," where
all available threads are blocked waiting for I/O.

### What Problem Async Programming Solves

The primary challenge in software performance is often not the CPU speed, but the time spent
waiting for external operations—such as database queries, network requests, or disk I/O. In a
synchronous world, a thread (the unit of execution) must wait for these operations to complete
before it can proceed.

If this happens on a UI thread, the application freezes. If it happens on a web server, a thread is
held captive, unable to process other incoming requests. Asynchronous programming solves this by
allowing a thread to yield control back to the system while waiting for an operation to finish,
enabling it to perform other work in the meantime.

Let's take a real-world example for this topic. Imagine a restaurant, a **waiter** represents a
**thread**, and his work represents the execution of the code, by executing tasks. While he's
serving one table, he can't serve another at the exact same time. A CPU works the same way,
handling one task at a time, but so incredibly fast we don't even notice.

Now, a customer places an order. In a synchronous scenario, the waiter walks to the kitchen balcony
to place the order and just stands there waiting for the dish to be ready, so he can deliver the
dish to the customer. During that time, he's doing nothing else, no new orders, no serving another
table. It's like the "restaurant UI is blocked".

In an asynchronous scenario, the waiter places the order in the kitchen and immediately is
"relieved" to go back to work, serving other tables, taking new orders, staying productive.
He doesn't "freeze" waiting.

When the dish is ready, it doesn't have to be the very same waiter who delivers it. Any available
waiter can pick it up and serve the customer. Just like a thread manager would do, it delegates
any available thread to pick up the completed task and continue the execution of the code.

### Introduction to Task

A `Task` represents a "promise" or a "future" operation that will complete at some point.

- `Task`: Represents an operation that returns no value.
- `Task<T>`: Represents an operation that will eventually return a value of type `T`.

Tasks are managed by the **Task Scheduler**, which decides which thread should run which task.
Unlike low-level threads, Tasks provide a high-level API for composition (like `Task.WhenAll`,
`Task.WhenAny`, and continuations).

Let's see some examples: Lab 1 and 2

### The Difference Between Parallel and Async Code

While both are used to improve performance, they address different bottlenecks:

- **Asynchronous Programming** is about **concurrency without necessarily using more threads**. it
  focuses on non-blocking I/O. It allows a single thread to manage multiple operations by
  "awaiting" their completion without idling.

Just like our restaurant, it can have just one single waiter and work with async. One waiter doing
multiple tasks, not necessarily in parallel.

- **Parallel Programming** is about **executing multiple computations simultaneously**, usually
  across multiple CPU cores. It is intended for CPU-bound tasks (like heavy mathematical
  calculations or image processing) where you want to split a large job into smaller chunks to run
  at the same time.

In the restaurant analogy, this is the kitchen preparing a complex dish.
The chef doesn’t cook everything alone—instead, different cooks handle different
parts (meat, sauce, garnish) at the same time, and everything comes together at the end.
Parallelism is about dividing work and doing it simultaneously to finish faster.

In short: Async is about *waiting* efficiently; Parallel is about *doing* multiple things at once.

### Async and Await Keywords

The `async` and `await` keywords are syntactic sugar introduced in C# 5.0 to simplify asynchronous
code:

- **`async`**: This modifier marks a method as asynchronous. It enables the use of `await` within
  the method and instructs the compiler to generate a state machine to handle the execution flow.
- **`await`**: This operator is applied to a `Task`. It suspends the execution of the method until
  the task completes. Critically, it does not block the thread; it yields control. When the task
  finishes, the method resumes.

### Be careful when using Task

Just adding `async` and `await` doesn’t automatically make your code truly asynchronous.
If used incorrectly, you can end up with synchronous behavior disguised as async code.

Async code only brings benefits when you avoid blocking and properly await non-blocking operations.
Misusing patterns like `.Wait()`, `.Result`, or even `await` in the wrong place can cancel those
benefits.

Async is not just syntax, it’s about how you structure execution and avoid blocking the flow.

In Labs 3, 4, and 5, we compare `Task.WaitAll` and `Task.WhenAll`, and show how improper use of
`await`
can still lead to blocking, effectively making your code behave synchronously.

### Why Never Use "void" return type on Async Methods

You should almost always use `Task` instead of `void` for asynchronous methods that don't return a
value.

- **`async Task`**: Allows the caller to `await` the method, handle exceptions, and know when it
  has finished. This will seldom crash the entire process because any exception remains managed
  by .NET runtime infrastructure.
- **`async void`**: This is "fire and forget." The caller has no way to track progress or wait for
  completion. Most dangerously, **exceptions thrown in an `async void` method cannot be caught by
  the caller**; they often crash the entire process.

In our restaurant analogy, the `async void` is like the waiter shouting an order to the kitchen...
and then walking away with no record of it.

- The waiter doesn't track the order
- No one knows when (or if) the dish will be ready
- The kitchen doesn't know who, or where, to report the dish is ready
- In the worst scenario, if the kitchen catches fire, instead of informing someone, the whole
  restaurant just goes down

The only valid exception for `async void` is for asynchronous event handlers (e.g., a button click
in a UI framework), as the event signature requires a void return.

Methods that don’t return a value should almost always return `Task`, not `void`.
Methods that return a value should return `Task<T>`.
`ValueTask` is an optimization and should be used only in specific scenarios.

- It avoids allocations in high-performance scenarios
- But it adds complexity (can only be awaited once, harder to use correctly)
- In most cases, `Task` / `Task<T>` is the right default

Let's take a look at Lab 6 and 7.

### Handling Exceptions and Propagation

In asynchronous methods, exceptions are captured and placed inside the returned `Task` object.

- **Propagation**: When you `await` a task, if that task faulted, the exception is re-thrown at the
  point of the `await`. This allows you to use standard `try-catch` blocks as if the code were
  synchronous.
- **Task.WhenAll**: When awaiting multiple tasks simultaneously, if several fail, the `await` will
  only throw the first exception. To examine all exceptions, you can inspect the `Task.Exception`
  property (which is an `AggregateException`) of the task returned by `WhenAll`.
- **Capture**: Always wrap your `await` calls in `try-catch` to handle potential failures
  gracefully.

Let's see on Lab 8

### Cancellation Token and Stopping a Task

In many scenarios, you may need to stop a long-running task (e.g., the user cancelled the operation
or a timeout occurred). .NET uses the `CancellationTokenSource` and `CancellationToken` mechanism
for **cooperative cancellation**:

1. **`CancellationTokenSource`**: The object that triggers the cancellation.
2. **`CancellationToken`**: The token passed into the asynchronous method.

The task must periodically check the token using `token.ThrowIfCancellationRequested()` or pass the
token to other async methods (like `Task.Delay(1000, token)`). Cancellation is "cooperative"
because the task is responsible for checking the token and shutting down gracefully; the system
does not forcibly kill the execution.

Let's see on Lab 9

### Understanding Continuation

A **Continuation** is a piece of code that runs after an asynchronous operation completes. In the
`async/await` pattern, everything following an `await` statement is effectively a continuation.

Behind the scenes, the compiler uses `Task.ContinueWith` to attach these continuations. When a task
completes, the "state machine" transitions to the next state, executing the remaining code. This
allows for complex sequences of operations without blocking threads.

We will see on Lab 10

### How to Obtain Result from a Task

The correct way to get a result from a `Task<T>` is using the `await` keyword:

```csharp
T result = await myTask;
```

Avoid using `.Result` or `.Wait()`. These properties/methods block the calling thread until the
task is complete, which completely defeats the purpose of async programming and frequently leads to
**deadlocks**, especially in environments with a SynchronizationContext (like ASP.NET or legacy UI
apps).

### Nested Async Operations

Asynchronous operations can be nested and composed. An `async` method can call other `async`
methods, creating a chain of tasks. The `await` keyword ensures that the chain is followed
correctly. It is important to "async all the way up"—meaning you should avoid mixing synchronous
and asynchronous code in the same call stack to prevent blocking and deadlocks.

### Handling Success and Failure

Beyond `try-catch`, you can manage success and failure by inspecting the task's state:

- **`IsCompletedSuccessfully`**: Returns true if the task finished without errors or cancellation.
- **`IsFaulted`**: Returns true if an exception occurred.
- **`IsCanceled`**: Returns true if the task was stopped via a `CancellationToken`.

Using these properties can be useful when you want to handle results without throwing/catching
exceptions immediately, or when processing collections of tasks.



