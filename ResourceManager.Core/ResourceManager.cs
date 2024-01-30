using System.Diagnostics;
using System.Runtime.Versioning;

namespace ResourceManager.Core;

[SupportedOSPlatform("windows")]
public class ResourceManager : IDisposable
{
    private const int MIN_MEMORY_THRESHOLD_BYTES = 1024 * 1024 * 200; // 200 MB
    private const int DELAY_BETWEEN_MEMORY_CHECKS = 50;
    private readonly string _appPath;
    private readonly int _memoryThresholdBytes;
    private readonly List<ProjectParameters> _projects;
    private readonly SemaphoreSlim _semaphore;
    private readonly PerformanceCounter _availableMemoryBytes = new("Memory", "Available Bytes");
    private readonly Logger<ResourceManager> _log = new("log")
    {
    #if DEBUG
        IsEnabled = true
    #else
        IsEnabled = false
    #endif
    };

    public ResourceManager(
        string appPath,
        string projectsParametersPath,
        int maxGlobalThreads,
        int memoryThresholdBytes)
    {
        _log.Log($"Initializing resource manager with app path '{appPath}', " +
            $"projects parameters path '{projectsParametersPath}', " +
            $"max global threads '{maxGlobalThreads}' and memory threshold '{memoryThresholdBytes}'.");

        _appPath =
            string.IsNullOrWhiteSpace(appPath)
                || !File.Exists(appPath)
                || !Path.GetExtension(appPath).Equals(".exe", StringComparison.OrdinalIgnoreCase)
            ? throw new ArgumentException(
                $"'{nameof(appPath)}' is not a valid path to an existing file.",
                nameof(appPath))
            : appPath;

        if (string.IsNullOrWhiteSpace(projectsParametersPath) || !File.Exists(projectsParametersPath))
        {
            throw new ArgumentException(
                $"'{nameof(projectsParametersPath)}' is not a valid path to an existing file.",
                nameof(projectsParametersPath));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(maxGlobalThreads, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(memoryThresholdBytes, MIN_MEMORY_THRESHOLD_BYTES);

        _semaphore = new SemaphoreSlim(maxGlobalThreads);

        _memoryThresholdBytes = memoryThresholdBytes;

        var json = File.ReadAllText(projectsParametersPath);

        _projects = ProjectsList.FromJson(json)?.Projects
            ?? throw new Exception($"Failed to deserialize projects from " +
                $"'{projectsParametersPath}' file.");

        _log.Log($"Resource manager initialized successfully. Number of " +
            $"projects loaded: {_projects.Count}.");
    }

    public async Task ExecuteProjectsAsync()
    {
        _log.Log($"Setting up projects.");
        _log.Log($"'Memory left: {_availableMemoryBytes.RawValue / 1024 / 1024} MB");

        var tasks = new List<Task>();

        foreach (var project in _projects)
        {
            _log.Log($"'{project.Id}' - Starting project with memory count '{project.MemoryCount}', " +
                $"app timeout '{project.AppTimeout}', try count '{project.TryCount}'  " +
                $"and max threads '{project.MaxThreads}'.");

            var task = Task.Run(async () =>
            {
                await ExecuteProjectAsync(project);
            });

            tasks.Add(task);

            _log.Log($"'{project.Id}' - Project set up.");
        }

        _log.Log($"Waiting for all projects to finish.");

        await Task.WhenAll(tasks);
    }

    private async Task ExecuteProjectAsync(ProjectParameters project)
    {
        _log.Log($"'{project.Id}' - Executing project.");

        var tasks = new List<Task>();

        for (int i = 0; i < project.TryCount; i++)
        {
            var tryId = i + 1;

            _log.Log($"'{project.Id}' - try '{tryId}' started.");
            
            tasks.Add(Task.Run(() => ExecuteApp(project, tryId)));
        }

        _log.Log($"'{project.Id}' - Waiting for all tries to finish.");

        await Task.WhenAll(tasks);

        _log.Log($"'{project.Id}' - Project executed.");
    }

    private void ExecuteApp(ProjectParameters project, int instanceIndex)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _appPath,
            Arguments = $"--app-timeout {project.AppTimeout} --memory-count {project.MemoryCount} --instance-id {project.Id}_{instanceIndex}",
            CreateNoWindow = true,
            WorkingDirectory = "../../../../"
        };

        using var process = new Process { StartInfo = processStartInfo };
        var memoryIsEnough = _availableMemoryBytes.RawValue > _memoryThresholdBytes;

        _log.Log($"'{project.Id}' [{instanceIndex}] - Memory left: {_availableMemoryBytes.RawValue / 1024 / 1024} MB");

        _semaphore.Wait();

        _log.Log($"'{project.Id}' [{instanceIndex}] - Semaphore acquired.");

        try
        {
            _log.Log($"'{project.Id}' [{instanceIndex}] - Trying to execute app.");

            while (!memoryIsEnough)
            {
                _log.Log($"'{project.Id}' [{instanceIndex}] - Memory is not enough, waiting.");

                Thread.Sleep(DELAY_BETWEEN_MEMORY_CHECKS);

                memoryIsEnough = _availableMemoryBytes.RawValue > _memoryThresholdBytes;
            }

            _log.Log($"'{project.Id}' [{instanceIndex}] - Memory is enough, executing app.");

            process.Start();

            _log.Log($"'{project.Id}' [{instanceIndex}] - App execution started.");

            process.WaitForExit();

            _log.Log($"'{project.Id}' [{instanceIndex}] - App execution finished. Exit code: '{process.ExitCode}'.");
        }
        catch (Exception ex)
        {
            _log.Log($"'{project.Id}' [{instanceIndex}] - Failed to execute project. Exception: '{ex}'");
        }
        finally
        {
            var count = _semaphore.Release();
            
            _log.Log($"'{project.Id}' [{instanceIndex}] - Semaphore released. Semaphore count: {count}.");
        }

    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _semaphore.Dispose();
    }
}
