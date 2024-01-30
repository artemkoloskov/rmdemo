using ConsoleApp;

try
{
    ArgumentParser.CheckArgs(args);

    int _appTimeout = ArgumentParser.ParseAppTimeout(args);
    int _memoryCount = ArgumentParser.ParseMemoryCount(args);
    string _instanceId = ArgumentParser.ParseInstanceId(args);
    bool _shouldLog = ArgumentParser.ParseLog(args);

    using var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(_appTimeout));

    var memory = AllocateMemory(_memoryCount, _instanceId, _shouldLog);
    var rnd = new Random();

    while (!cancellationToken.IsCancellationRequested)
    {
        memory[rnd.Next(0, memory.Length - 1)]++;
    }

    Log($"[{_instanceId}] Allocated memory: {memory.Length / 1024 / 1024} MB.", _shouldLog);
}
catch (Exception ex)
{
    Log($"[{ArgumentParser.ParseInstanceId(args)}] {ex.Message}", ArgumentParser.ParseLog(args));
}

static byte[] AllocateMemory(int memoryCount, string instanceId, bool shouldLog)
{
    var memory = new byte[memoryCount * 1024 * 1024];

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
