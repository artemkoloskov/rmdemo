using System.Runtime.Versioning;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using ResourceManager.Benchmark;

[SupportedOSPlatform("windows")]
internal class Program
{
    private static void Main(string[] args)
    {
        SaveSettingsFile(args);

        BenchmarkRunner.Run<Benchmarks>();
    }

    private static void SaveSettingsFile(string[] args)
    {
        ArgumentParser.CheckArgs(args);

        var _filePath = "settings.json";

        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Dispose();
        }

        BenchmarksSettings settings = new()
        {
            AppPath = ArgumentParser.ParseAppPath(args),
            ProjectsParametersPath = ArgumentParser.ParseProjectsParametersPath(args),
            MaxGlobalThreads = ArgumentParser.ParseMaxGlobalThreads(args),
            MemoryThresholdMb = ArgumentParser.ParseMemoryThresholdMb(args),
            EnableLogging = ArgumentParser.ParseEnableLogging(args),
        };

        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

        File.WriteAllText(_filePath, json);
    }
}
