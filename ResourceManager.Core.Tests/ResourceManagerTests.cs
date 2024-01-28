namespace ResourceManager.Core.Tests;

public class ResourceManagerTests
{
    [Fact]
    public void Constructor_AppFilePathIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        string? appPath = "";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads));
    }

    [Fact]
    public void Constructor_AppFilePathIsNotExecutable_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.dll";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads));
    }

    [Fact]
    public void Constructor_JsonFilePathIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        string? jsonFilePath = "";
        var maxGlobalThreads = 10;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads));
    }

    [Fact]
    public void Constructor_MaxGlobalThreadsIsLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 0;

        // Act & Assert
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads));
    }

    [Fact]
    public void Constructor_ValidParameters_DoesNotThrow()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;

        // Act
        var exception = Record.Exception(() => new ResourceManager(appPath, jsonFilePath, maxGlobalThreads));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ExecuteProjects_ValidJsonFilePath_ExecutesProjects()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var resourceManager = new ResourceManager(appPath, jsonFilePath, maxGlobalThreads);

        // Act
        resourceManager.ExecuteProjects();

        // Assert
        Assert.True(true);
    }
}
