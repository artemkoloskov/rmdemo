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
        return JsonConvert.DeserializeObject<ProjectsList>(json);
    }
}

/// <summary>
/// Represents parameters of a project. For JSON deserialization.
/// </summary>
public class ProjectParameters
{
    public int? MemoryCount { get; set; }
    public int? AppTimeout { get; set; }
    public int? TryCount { get; set; }
    public int? MaxThreads { get; set; }
}
