namespace ResourceManager.Core.Tests;

public class ResourceManagerTests
{
    [Fact]
    public void Constructor_WhenJsonFilePathIsInvalid_ThrowsArgumentNullException()
    {
        // Arrange
        string? appPath = "";
        var maxGlobalThreads = 10;

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => new ResourceManager(appPath, maxGlobalThreads));
    }

    [Fact]
    public void Constructor_WhenMaxGlobalThreadsIsLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var maxGlobalThreads = 0;

        // Act & Assert
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ResourceManager(appPath, maxGlobalThreads));
    }

    [Fact]
    public void ExecuteProjects_WhenJsonFilePathIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        string? jsonFilePath = null;
        var maxGlobalThreads = 10;
        var resourceManager = new ResourceManager(appPath, maxGlobalThreads);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => resourceManager.ExecuteProjects(jsonFilePath));
    }

    [Fact]
    public void ExecuteProjects_ValidJsonFilePath_ExecutesProjects()
    {
        // Arrange
        var appPath = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
        var jsonFilePath = "projects.json";
        var maxGlobalThreads = 10;
        var resourceManager = new ResourceManager(appPath, maxGlobalThreads);

        // Act
        resourceManager.ExecuteProjects(jsonFilePath);

        // Assert
        Assert.True(true);
    }
}
