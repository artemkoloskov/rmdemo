namespace ResourceManager.Benchmark;

public class BenchmarksSettings
{
    public string? AppPath { get; set; }
    public string? ProjectsParametersPath { get; set; }
    public int MaxGlobalThreads { get; set; }
    public int MemoryThresholdMb { get; set; }
    public bool EnableLogging { get; set; }
}
