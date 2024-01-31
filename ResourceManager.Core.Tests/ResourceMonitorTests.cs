using System.Runtime.Versioning;

namespace ResourceManager.Core.Tests;

[SupportedOSPlatform("windows")]
public class ResourceMonitorTests
{
    [Fact]
    public void EnoughResources_EnoughProcessorTimeNotEnoughMemory_ReturnsFalse()
    {
        // Arrange
        var resourceMonitor = new ResourceMonitor(201 * 1024 * 1024, 5);
        Task.Run(PutLoadOnMemory);
        Thread.Sleep(3000);

        // Act
        var enoughResources = resourceMonitor.EnoughResources(2000);

        // Assert
        Assert.False(enoughResources);
    }

    [Fact]
    public void EnoughResources_EnoughMemoryNotEnoughProcessorTime_ReturnsFalse()
    {
        // Arrange
        var resourceMonitor = new ResourceMonitor(201 * 1024 * 1024, 5);
        Task.Run(PutLoadOnProcessor);
        Task.Run(PutLoadOnProcessor);
        Task.Run(PutLoadOnProcessor);
        Task.Run(PutLoadOnProcessor);
        Task.Run(PutLoadOnProcessor);
        Thread.Sleep(1000);

        // Act
        var enoughResources = resourceMonitor.EnoughResources(2000);

        // Assert
        Assert.False(enoughResources);
    }

    [Fact]
    public void EnoughResources_NoLoadOnSystem_ReturnsTrue()
    {
        // Arrange
        var resourceMonitor = new ResourceMonitor(201 * 1024 * 1024, 5);
        Thread.Sleep(1000);

        // Act
        var enoughResources = resourceMonitor.EnoughResources(2000);

        // Assert
        Assert.True(enoughResources);
    }

    [Fact]
    public void AverageProcessorTimeInUse_LoadOnSystem_ReturnsValue()
    {
        // Arrange
        var resourceMonitor = new ResourceMonitor(201 * 1024 * 1024, 5);
        Task.Run(() => PutLoadOnProcessor());
        Thread.Sleep(1000);
        _ = resourceMonitor.EnoughResources(2000);

        // Act
        var averageProcessorTimeInUse = resourceMonitor.AverageProcessorTimeInUse();

        // Assert
        Assert.True(averageProcessorTimeInUse > 0);
    }

    [Fact]
    public void AverageMemoryInUseMb_LoadOnSystem_ReturnsValue()
    {
        // Arrange
        var resourceMonitor = new ResourceMonitor(201 * 1024 * 1024, 5);
        Task.Run(() => PutLoadOnMemory());
        Thread.Sleep(1000);
        _ = resourceMonitor.EnoughResources(2000);

        // Act
        var averageMemoryInUseMb = resourceMonitor.AverageMemoryInUseMb();

        // Assert
        Assert.True(averageMemoryInUseMb > 0);
    }
    
    [Fact]
    public void TotalProcessingTime_LoadOnSystem_ReturnsValue()
    {
        // Arrange
        var resourceMonitor = new ResourceMonitor(201 * 1024 * 1024, 5);
        Task.Run(() => PutLoadOnProcessor());
        Thread.Sleep(1000);

        // Act
        var totalProcessingTime = resourceMonitor.TotalProcessingTime();

        // Assert
        Assert.True(totalProcessingTime > 1000, $"Total processing time is {totalProcessingTime} ms.");
    }

    private static void PutLoadOnProcessor()
    {
        var rnd = new Random();
        float result = 0;
        while(true)
        {
            result += rnd.Next(0, 1000) / rnd.Next(1, 1000) / rnd.Next(1, 1000);
        }
    }

    private static void PutLoadOnMemory()
    {
        var rnd = new Random();
        var list = new List<byte[]>();
        
        for (int i = 0; i < 10; i++)
        {
            var array = new byte[1024 * 1024 * 2000];
            list.Add(array);
        }

        while(true)
        {
            var index = rnd.Next(0, list.Count);
            var index2 = rnd.Next(0, 1024 * 1024 * 2000);
            list[index][index2] = 1;
        }
    }
}
