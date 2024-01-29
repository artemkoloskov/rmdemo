# RMDemo

This is a repo with a solution to the following problem:

There exists a windows console application that accepts arguments:

```console
--app-timeout [integer]
--memory-count [integer]
```

Implement is a C# class called `ResourceManager`, that makes possible to read a
JSON file with a following structure:

```json
{
    "Projects": [
        { "MemoryCount": 100, "AppTimeout": 100, "TryCount": 100, "MaxThreads": 10 },
        { "MemoryCount": 200, "AppTimeout": 200, "TryCount": 100, "MaxThreads": 10 }
    ]
}
```

When file is loaded, the `ResourceManager` is also provided with a parameter
`MaxGlobalThreads`.

A "project" from the JSON file is a collection of parameters that are used by the
`ResourceManager` to execute a `ConsoleApp`  `TryCount` number of times with
`MemoryCount` and `AppTimeout` provided as CLI arguments, using no more the
`MaxThreads` number of threads at any given point in time.

The `ResourceManager` has to execute all "projects" in the JSON file as quickly
as possible without using more than `MaxGlobalThreads` number of threads
at any moment of time, while leaving the system responsive without hogging all
resources.
