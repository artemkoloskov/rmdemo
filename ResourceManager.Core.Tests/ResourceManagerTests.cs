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
        var processorTimeThreshold = 31f;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold));
    }

    [Fact]
    public void Constructor_AppFilePathIsNotExecutable_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.dll";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB
        var processorTimeThreshold = 31f;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold));
    }

    [Fact]
    public void Constructor_JsonFilePathIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        string? jsonFilePath = "";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB
        var processorTimeThreshold = 31f;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold));
    }

    [Fact]
    public void Constructor_MaxGlobalThreadsIsLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 0;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB
        var processorTimeThreshold = 31f;

        // Act & Assert
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold));
    }

    [Fact]
    public void Constructor_MemoryThresholdIsLessThanMinimumRequired_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 0;
        var processorTimeThreshold = 31f;

        // Act & Assert
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold));
    }

    [Fact]
    public void Constructor_ProcessorTimeThresholdIsLessThanMinimumRequired_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB
        var processorTimeThreshold = 0f;

        // Act & Assert
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold));
    }

    [Fact]
    public void Constructor_ValidParameters_DoesNotThrow()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var memoryThreshold = 1024 * 1024 * 1024; // 1 GB
        var processorTimeThreshold = 31f;

        // Act
        var exception = Record.Exception(() => new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold));

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
        var processorTimeThreshold = 31f;
        var resourceManager = new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold);
        resourceManager.EnableReporting("Z:\\temp");

        // Act
        resourceManager.ExecuteProjects();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void ExecuteProjects_HeavyLoadValidJsonFilePath_ExecutesProjects()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "testdataset.json";
        var maxGlobalThreads = 20;
        var memoryThreshold = 1024 * 1024 * 201;
        var processorTimeThreshold = 6f;
        var resourceManager = new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold);
        resourceManager.EnableReporting("Z:\\temp");

        // Act
        resourceManager.ExecuteProjects();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void EnableReporting_ValidJsonFilePathAndInvalidReportPath_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 15;
        var memoryThreshold = 1024 * 1024 * 1001;
        var processorTimeThreshold = 31f;
        var resourceManager = new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => resourceManager.EnableReporting(""));
    }

    [Fact]
    public void EnableReporting_ValidJsonFilePathAndValidReportPath_DoesNotThrow()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "easyprojects.json";
        var maxGlobalThreads = 15;
        var memoryThreshold = 1024 * 1024 * 1001;
        var processorTimeThreshold = 31f;
        var resourceManager = new ResourceManager(
            appPath,
            jsonFilePath,
            maxGlobalThreads, 
            memoryThreshold,
            processorTimeThreshold);

        // Act
        var exception = Record.Exception(() => resourceManager.EnableReporting("Z:\\temp"));

        // Assert
        Assert.Null(exception);
        resourceManager.ExecuteProjects();
    }
}
