using ConsoleApp;

try
{
    ArgumentParser.CheckArgs(args);

    int _appTimeout = ArgumentParser.ParseAppTimeout(args);
    int _memoryCount = ArgumentParser.ParseMemoryCount(args);
    string _instanceId = ArgumentParser.ParseInstanceId(args);

    var cancellationToken = new CancellationTokenSource();
    var _timer = new Timer(
        _ => cancellationToken.Cancel(),
        null,
        TimeSpan.FromMilliseconds(_appTimeout),
        Timeout.InfiniteTimeSpan);

    var memory = AllocateMemory(_memoryCount);

    Log($"[{_instanceId}] Allocated memory: {memory.Length/1024/1024} MB.");

    long counter = 0;

    while (!cancellationToken.IsCancellationRequested)
    {
        counter++;
    }
}
catch (Exception ex)
{
    Log(ex.Message);
}

static byte[] AllocateMemory(int _memoryCount)
{
    var memory = new byte[_memoryCount * 1024 * 1024];

    for (int i = 0; i < memory.Length; i++)
    {
        memory[i] = 1;
    }

    return memory;
}

static void Log(string message)
{
    var _filePath = "ConsoleApp.log";

    if (!File.Exists(_filePath))
    {
        File.Create(_filePath).Dispose();
    }

    using StreamWriter writer = new(_filePath, true);

    writer.WriteLine($"[{DateTime.Now}] - {message}");
}
