using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace ResourceManager.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
[SupportedOSPlatform("windows")]
public class Benchmarks
{
    private readonly Core.ResourceManager _resourceManager;

    public Benchmarks()
    {
        var settings = LoadSettingsFromFile();

        _resourceManager = new(
            settings.AppPath
                ?? throw new ArgumentNullException(nameof(settings.AppPath)),
            settings.ProjectsParametersPath
                ?? throw new ArgumentNullException(nameof(settings.ProjectsParametersPath)),
            settings.MaxGlobalThreads,
            settings.MemoryThresholdMb * 1024 * 1024,
            settings.ProcessorTimeThreshold
        );

        if (settings.EnableLogging)
        {
            _resourceManager.EnableLogging();
        }

        _resourceManager.EnableReporting("Z:\\temp");
    }

    private static BenchmarksSettings LoadSettingsFromFile()
    {
        var filelPath = Path.GetFullPath("../../../../settings.json");

        if (!File.Exists(filelPath))
        {
            throw new FileNotFoundException($"Settings file {filelPath} not found");
        }

        var json = File.ReadAllText(filelPath);

        return JsonConvert.DeserializeObject<BenchmarksSettings>(json)
            ?? throw new JsonException("Settings file is not valid");
    }

    [Benchmark]
    public void Run()
    {
        _resourceManager.ExecuteProjects();
    }
}
