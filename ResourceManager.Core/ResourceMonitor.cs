using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ResourceManager.Core;

[SupportedOSPlatform("windows")]
public class ResourceMonitor
{
    private const int DELAY_BETWEEN_CHECKS = 50;
    private const float MIN_PROCESSOR_TIME_THRESHOLD = 30f;
    private const int MIN_MEMORY_THRESHOLD_BYTES = 1024 * 1024 * 200;
    private static readonly PerformanceCounter _availableMemoryBytes =
        new("Memory", "Available Bytes");
    private static readonly PerformanceCounter _processorTime =
        new("Processor", "% Processor Time", "_Total");

    private static readonly Stopwatch _stopwatch = new();
    private static readonly Logger<ResourceMonitor> _log = new("log")
    {
#if DEBUG
        IsEnabled = true
#else
        IsEnabled = false
#endif
    };

    private static int _memoryThresholdBytes = MIN_MEMORY_THRESHOLD_BYTES;
    private static float _processorTimeThreshold = MIN_PROCESSOR_TIME_THRESHOLD;
    private static int _processorTimeCallsCount = 0;
    private static float _processorTimeSum = 0;
    private static int _memoryCallsCount = 0;
    private static long _memorySumMb = 0;


    static ResourceMonitor()
    {
        _stopwatch.Start();
        _processorTime.NextValue();

        _log.Log("ResourceMonitor initialized.");
    }

    public static void Configure(int memoryThresholdBytes, float processorTimeThreshold)
    {
        _log.Log($"Configuring ResourceMonitor with " +
            $"memoryThresholdBytes: {memoryThresholdBytes} and " +
            $"processorTimeThreshold: {processorTimeThreshold}");

        ArgumentOutOfRangeException.ThrowIfLessThan(memoryThresholdBytes, MIN_MEMORY_THRESHOLD_BYTES);
        ArgumentOutOfRangeException.ThrowIfLessThan(processorTimeThreshold, MIN_PROCESSOR_TIME_THRESHOLD);

        _memoryThresholdBytes = memoryThresholdBytes;
        _processorTimeThreshold = processorTimeThreshold;
    }

    public static void WaitForEnoughResources(int memoryToAllocateMb)
    {
        _log.Log($"Waiting for enough resources to allocate " +
            $"{memoryToAllocateMb} MB.");

        while (!EnoughResources(memoryToAllocateMb))
        {
            Thread.Sleep(DELAY_BETWEEN_CHECKS);
        }

        _log.Log($"Enough resources to allocate " +
            $"{memoryToAllocateMb} MB.");
    }

    public static bool EnoughResources(int memoryToAllocateMb)
    {
        return EnoughProcessorTime() && EnoughMemory(memoryToAllocateMb);
    }

    public static float AverageProcessorTimeInUse()
    {
        if (_processorTimeCallsCount == 0)
        {
            _log.Log("_processorTimeCallsCount is 0.");

            return 0;
        }

        var result = _processorTimeSum / _processorTimeCallsCount;

        _log.Log($"Average processor time in use: {result}%.");

        return result;
    }

    public static long AverageMemoryInUseMb()
    {
        if (_memoryCallsCount == 0)
        {
            _log.Log("_memoryCallsCount is 0.");

            return 0;
        }

        var averageFreeMemory = _memorySumMb / _memoryCallsCount;
        var result = GetTotalMemory() - averageFreeMemory;

        _log.Log($"Average memory in use: {result} MB.");

        return result;
    }

    public static long TotalProcessingTime()
    {
        var result = _stopwatch.ElapsedMilliseconds;

        _log.Log($"Total processing time: {result} ms.");

        return result;
    }

    private static bool EnoughProcessorTime()
    {
        var availableProcessorTime = 100 - GetProcessorTime();

        _log.Log($"Available processor time: {availableProcessorTime}%. " +
            $"Processor time threshold: {_processorTimeThreshold}%.");

        var result = availableProcessorTime > _processorTimeThreshold;

        _log.Log($"Enough processor time: {result}.");

        return result;
    }

    private static bool EnoughMemory(int memoryToAllocateMb)
    {
        var availableMemoryMb = GetAvailableMemoryMb();
        var memoryThresholdMb = _memoryThresholdBytes / 1024 / 1024;

        _log.Log($"Available memory: {availableMemoryMb} MB. " +
            $"Memory threshold: {memoryThresholdMb} MB. " +
            $"Memory to allocate: {memoryToAllocateMb} MB.");

        return availableMemoryMb > memoryThresholdMb + memoryToAllocateMb;
    }

    private static long GetAvailableMemoryMb()
    {
        var memory = _availableMemoryBytes.RawValue / 1024 / 1024;
        _memoryCallsCount++;
        _memorySumMb += memory;

        return memory;
    }

    private static float GetProcessorTime()
    {
        float processorTime = _processorTime.NextValue();
        _processorTimeCallsCount++;
        _processorTimeSum += processorTime;

        return processorTime;
    }

    private static long GetTotalMemory()
    {
        GetPhysicallyInstalledSystemMemory(out long memoryKb);

        return memoryKb / 1024;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
}
