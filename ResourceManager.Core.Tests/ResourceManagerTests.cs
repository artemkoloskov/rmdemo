using System.Runtime.Versioning;

namespace ResourceManager.Core.Tests;

[SupportedOSPlatform("windows")]
public class ResourceManagerTests
{
    [Fact]
    public void Constructor_AppFilePathIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        string? appPath = "";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads, memoryThreshold));
    }

    [Fact]
    public void Constructor_AppFilePathIsNotExecutable_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.dll";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads, memoryThreshold));
    }

    [Fact]
    public void Constructor_JsonFilePathIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        string? jsonFilePath = "";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads, memoryThreshold));
    }

    [Fact]
    public void Constructor_MaxGlobalThreadsIsLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 0;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB

        // Act & Assert
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads, memoryThreshold));
    }

    [Fact]
    public void Constructor_MemoryThresholdIsLessThanMinimumRequired_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 0;

        // Act & Assert
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads, memoryThreshold));
    }

    [Fact]
    public void Constructor_ValidParameters_DoesNotThrow()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB

        // Act
        var exception = Record.Exception(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads, memoryThreshold));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ExecuteProjects_ValidJsonFilePath_ExecutesProjects()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 15;
        var memoryThreshold = 1024 * 1024 * 1001; 
        var resourceManager = new ResourceManager(appPath, jsonFilePath, maxGlobalThreads, memoryThreshold);

        // Act
        resourceManager.ExecuteProjects();

        // Assert
        Assert.True(true);
    }
}
