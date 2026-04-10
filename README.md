# C# Engineering Handbook

An educational repository designed to demonstrate modern C# language features through practical
examples and detailed technical documentation. This repo serves as a living handbook for engineers
looking to deepen their understanding of the .NET ecosystem.

## Overview

This repository contains a collection of deep-dive articles and corresponding sample projects. Each
topic is explored from both a theoretical perspective (in `content/`) and a practical
implementation perspective (in `examples/`).

### Covered Topics

- **Async Programming**: Mastering `async/await`, `Task`, and deadlock prevention.
- **Collections**: Deep dive into .NET collections and performance characteristics.
- **Delegates & Events**: Understanding functional constructs and event-driven patterns.
- **Disposable Pattern**: Proper resource management and `IDisposable` implementation.
- **LINQ**: Effective use of Language Integrated Query.
- **Span & Memory**: High-performance memory management using `Span<T>` and `Memory<T>`.
- **Flagged Enums**: Working with bitwise operations and the `[Flags]` attribute.

## Project Structure

```text
.
├── content/           # Technical documentation and articles (Markdown)
├── examples/          # Sample projects demonstrating C# features
│   ├── AsyncProgramming/
│   ├── Collections/
│   ├── ConfigureAwaitDeadlock/
│   ├── Delegates/
│   ├── Disposable/
│   ├── FlaggedEnum/
│   ├── Linq/
│   └── SpanMemory/
└── csharp-engineering-handbook.sln  # Main solution file
```

## Requirements

- **.NET 10 SDK** (or newer)
- **IDE**: JetBrains Rider (recommended) or Visual Studio 2022+ / VS Code with C# Dev Kit.
- **OS**: Windows (required for WPF examples).

## Setup & Run

### Clone the repository

```powershell
git clone https://github.com/[username]/csharp-engineering-handbook.git
cd csharp-engineering-handbook
```

### Build the solution

```powershell
dotnet build
```

### Run an example

To run a specific console-based example:

```powershell
dotnet run --project examples/Linq/Linq.csproj
```

For WPF-based examples (e.g., `AsyncProgramming`), it is recommended to open the solution in
**Rider** or **Visual Studio** and run the project from the IDE.

## License

> [!TODO]
> License file is missing. Please add a `LICENSE` file to the repository.

---
*This handbook is intended for educational purposes and may be updated with new C# features.*
