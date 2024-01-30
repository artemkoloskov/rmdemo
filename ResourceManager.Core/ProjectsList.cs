using Newtonsoft.Json;

namespace ResourceManager.Core;

/// <summary>
/// Represents a list of projects. For JSON deserialization.
/// </summary>
public class ProjectsList
{
    public List<ProjectParameters>? Projects { get; set; }

    public static ProjectsList? FromJson(string json)
    {
        var result = JsonConvert.DeserializeObject<ProjectsList>(json);

        var i = 1;

        foreach (var project in result?.Projects!)
        {
            project.Id = i++;
        }

        return result;
    }
}

/// <summary>
/// Represents parameters of a project. For JSON deserialization.
/// </summary>
public class ProjectParameters
{
    [JsonIgnore]
    public int Id = 0;
    public int? MemoryCount { get; set; }
    public int? AppTimeout { get; set; }
    public int? TryCount { get; set; }
    public int? MaxThreads { get; set; }
}
