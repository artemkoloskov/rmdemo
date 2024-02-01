# RMDemo

This is a repo with a solution to the following problem:

There exists a Windows console application that accepts arguments:

```console
--app-timeout [integer]
--memory-count [integer]
```

Implement is a C# class called `ResourceManager`, that makes it possible to read a
JSON file with the following structure:

```json
{
    "Projects": [
        { "MemoryCount": 100, "AppTimeout": 100, "TryCount": 100, "MaxThreads": 10 },
        { "MemoryCount": 200, "AppTimeout": 200, "TryCount": 100, "MaxThreads": 10 }
    ]
}
```

When a file is loaded, the `ResourceManager` is also provided with a parameter
`MaxGlobalThreads`.

A "project" from the JSON file is a collection of parameters that are used by the
`ResourceManager` to execute a `ConsoleApp`  `TryCount` several times with
`MemoryCount` and `AppTimeout` provided as CLI arguments, using no more than 
the `MaxThreads` number of threads at any given point in time.

The `ResourceManager` has to execute all "projects" in the JSON file as quickly
as possible without using more than `MaxGlobalThreads` number of threads
at any moment while leaving the system responsive without hogging all
resources.

## Solution

The solution is implemented in C# using **.NET 8**. The solution consists of
four projects:

- `ConsoleApp` - a console application that accepts arguments:
  - `--app-timeout [integer]`
  - `--memory-count [integer]`
- `ResourceManager.Core` - a class library that implements the `ResourceManager`
    class.
- `ResourceManager.Core.Tests` - a class library that contains unit tests for the
    `ResourceManager.Core` classes.
- `ResourceManager.Benchmark` - a console application that uses the `ResourceManager`
    and **BenchmarkDotNet** to benchmark the `ResourceManager` class.

## Building

The solution can be built using **Visual Studio 2022** or **Visual Studio Code**,
or using the **dotnet** CLI (recommended):

```console
cd RMDemo
dotnet build -c Release
```

It is important to build the solution in the `Release` configuration, as the
`BenchmarkDotNet` will not work in the `Debug` configuration. Also, by default,
the benchmark will look for `ConsoleApp.exe` in the `ConsoleApp\bin\Release\net8.0`
directory.

## Running

The `ConsoleApp` can be run from the command line:

```console
cd ConsoleApp\bin\Release\net8.0
ConsoleApp.exe --app-timeout 100 --memory-count 100
```

The `ResourceManager.Benchmark` can be run from the command line:

```console
cd ResourceManager.Benchmark\bin\Release\net8.0
ResourceManager.Benchmark.exe --reports-path "C:\Temp\Reports"
```

The `ResourceManager.Benchmark` will run the `ResourceManager` with the following
default parameters:

- `--max-global-threads` = 20 - this is the maximum number of threads that the
    `ResourceManager` will use to execute the `ConsoleApp` instances.
- `--memory-threshold-mb` = 200 MB - this is the amount of memory that the
    `ResourceManager` will use to determine if the system is responsive or not.
- `--json-path` = `projects.json` - this will throw an exception if the file does
    not exist. This file is copied to the `ResourceManager.Benchmark\bin\Release\net8.0`
    directory when the solution is built.
- `--processor-time-threshold` = 5% - this is the percentage of the CPU time
    that the `ResourceManager` will use to determine if the system is responsive
    or not.
- `--reports-path` = `C:\Temp` - this will throw an exception if the directory
    does not exist.

It is recommended to run the `ResourceManager.Benchmark` with the custom
`--reports-path` parameter with an existing directory to avoid exceptions.
