using Newtonsoft.Json;

namespace ResourceManager.Core.Tests;

public class ProjectsListTests
{
    [Fact]
    public void FromJson_ValidJson_ReturnsProjectsList()
    {
        // Arrange
        var json = 
        @"{
            'Projects': [
                { 'MemoryCount': 100, 'AppTimeout': 100, 'TryCount': 100, 'MaxThreads': 10 },
                { 'MemoryCount': 200, 'AppTimeout': 200, 'TryCount': 100, 'MaxThreads': 10 }
            ]
        }";

        // Act
        var projectsList = ProjectsList.FromJson(json);

        // Assert
        Assert.NotNull(projectsList);
        Assert.NotNull(projectsList.Projects);
        Assert.Equal(2, projectsList.Projects.Count);
        Assert.Equal(100, projectsList.Projects[0].MemoryCount);
        Assert.Equal(100, projectsList.Projects[0].AppTimeout);
        Assert.Equal(100, projectsList.Projects[0].TryCount);
        Assert.Equal(10, projectsList.Projects[0].MaxThreads);
        Assert.Equal(200, projectsList.Projects[1].MemoryCount);
        Assert.Equal(200, projectsList.Projects[1].AppTimeout);
        Assert.Equal(100, projectsList.Projects[1].TryCount);
        Assert.Equal(10, projectsList.Projects[1].MaxThreads);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonReaderException()
    {
        // Arrange
        var json = "invalid json";

        // Act
        var exception = Record.Exception(() => ProjectsList.FromJson(json));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<JsonReaderException>(exception);
    }
}
