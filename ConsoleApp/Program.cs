using ConsoleApp;

try
{
    ArgumentParser.CheckArgs(args);

    int _appTimeout = ArgumentParser.ParseAppTimeout(args);
    int _memoryCount = ArgumentParser.ParseMemoryCount(args);
    string _instanceId = ArgumentParser.ParseInstanceId(args);
    bool _shouldLog = ArgumentParser.ParseLog(args);

    var cancellationToken = new CancellationTokenSource();
    var _timer = new Timer(
        _ => cancellationToken.Cancel(),
        null,
        TimeSpan.FromMilliseconds(_appTimeout),
        Timeout.InfiniteTimeSpan);

    new Thread(() => AllocateMemory(_memoryCount, _instanceId, _shouldLog)).Start();

    long counter = 0;

    while (!cancellationToken.IsCancellationRequested)
    {
        counter++;
    }

    Log($"[{_instanceId}] Counter: {counter}", _shouldLog);
}
catch (Exception ex)
{
    Log($"[{ArgumentParser.ParseInstanceId(args)}] {ex.Message}", ArgumentParser.ParseLog(args));
}

static byte[] AllocateMemory(int memoryCount, string instanceId, bool shouldLog)
{
    var memory = new byte[memoryCount * 1024 * 1024];

    for (int i = 0; i < memory.Length; i++)
    {
        memory[i] = 1;
    }
    
    Log($"[{instanceId}] Allocated memory: {memory.Length/1024/1024} MB.", shouldLog);

    return memory;
}

static void Log(string message, bool shouldLog)
{
    if (!shouldLog)
    {
        return;
    }

    var _filePath = "ConsoleApp.log";

    if (!File.Exists(_filePath))
    {
        File.Create(_filePath).Dispose();
    }

    using StreamWriter writer = new(_filePath, true);

    writer.WriteLine($"[{DateTime.Now}] - {message}");
}
