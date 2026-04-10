# Span<T> and Memory<T>

**Goal:** Work with slices of data in C# with maximum performance and zero copies.

---

## The Problem: Memory Copies

Traditional ways to handle "slices" of data (like `string.Substring()` or copying parts of an
array) involve allocating new memory and copying data. This creates pressure on the Garbage
Collector (GC) and slows down the application.

---

## 1. Span<T> – The High-Performance Window

**Definition:** A `Span<T>` is a **stack-only** type (`ref struct`) that represents a **contiguous
region of memory**.

* **What it points to:**
    * Arrays (`T[]`)
    * Stack memory (`stackalloc`)
    * Unmanaged/Native memory (via pointers)
* **Performance:** It has **zero allocation** and **zero copy** overhead. It's just a window into
  existing memory.

### Example:

```csharp
int[] numbers = { 10, 20, 30, 40, 50 };
// Create a slice starting at index 1 with length 3: [20, 30, 40]
Span<int> slice = numbers.AsSpan(1, 3);

slice[0] = 99; // Modifies the original array! numbers[1] is now 99.
```

### Constraints (The "ref struct" rule):

Because `Span<T>` can point to the **stack**, C# must ensure it never "escapes" to the heap where
it could outlive the stack frame it points to.

* **Cannot** be stored as a field in a class (only in other `ref struct`s).
* **Cannot** be used in `async` methods (across `await` boundaries).
* **Cannot** be boxed or captured in lambdas.

---

## 2. Memory<T> – The Heap-Friendly Wrapper

**Definition:** `Memory<T>` is a "promotable" version of `Span<T>`. It lives on the **heap**,
making it safe for long-term storage and asynchronous operations.

* **Usage:** Use it when you need to store a slice in a class or pass it across `await` points.
* **Conversion:** You can always get a `Span<T>` from a `Memory<T>` when you're ready to do
  synchronous work.

### Example:

```csharp
public class DataProcessor
{
    private Memory<byte> _buffer; // Safe to store as a field!

    public async Task ProcessAsync()
    {
        // ... some async work ...
        await Task.Delay(100);

        // Get a Span from Memory to do the actual processing
        Span<byte> span = _buffer.Span;
        ProcessSynchronously(span);
    }
}
```

---

## 3. ReadOnlySpan<T> and ReadOnlyMemory<T>

If you only need to **read** the data (like when processing strings or constant buffers), use the
`ReadOnly` variants. This prevents accidental modifications and is the modern way to handle string
slicing efficiently.

```csharp
string text = "Hello World";
ReadOnlySpan<char> world = text.AsSpan(6, 5); // "World"
```

---

## Comparison Table

| Feature           | Span<T>                   | Memory<T>              |
|-------------------|---------------------------|------------------------|
| **Storage**       | Stack-only (`ref struct`) | Heap-safe (`struct`)   |
| **Allocation**    | None                      | None                   |
| **Async Support** | No                        | Yes                    |
| **Class Fields**  | No                        | Yes                    |
| **Performance**   | Fastest                   | High (slight overhead) |
| **Common Use**    | Loops, parsing, math      | Buffers, Async APIs    |

---

## Why not ArraySegment<T>?

Before `Span<T>`, we had `ArraySegment<T>`. While it also provided a "window" into an array:

1. **It only worked with arrays.** It couldn't point to stack or unmanaged memory.
2. **It was clunky.** It didn't have the same optimized compiler support.
3. **Performance.** `Span<T>` is much closer to the metal, often as fast as raw pointers but with
   C# safety.

