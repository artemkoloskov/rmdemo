using System.Diagnostics;
using System.Runtime.Versioning;

namespace ResourceManager.Core;

[SupportedOSPlatform("windows")]
{
    private const int MIN_MEMORY_THRESHOLD_BYTES = 1024 * 1024 * 200; // 200 MB
    private const int DELAY_BETWEEN_MEMORY_CHECKS = 50;
    private readonly string _appPath;
    private readonly int _memoryThresholdBytes;
    private readonly List<ProjectParameters> _projects;
    private readonly SemaphoreSlim _semaphore;
    private readonly PerformanceCounter _availableMemoryBytes = new("Memory", "Available Bytes");
    private readonly Logger<ResourceManager> _log = new(".log")
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
        _log.Log($"Executing projects.");

        var tasks = new List<Task>();

        foreach (var project in _projects)
        {
            _log.Log($"'{project.Id}' - Starting project with memory count '{project.MemoryCount}', " +
                $"app timeout '{project.AppTimeout}', try count '{project.TryCount}'  " +
                $"and max threads '{project.MaxThreads}'.");

            var task = Task.Run(async () =>
            {
                await _semaphore.WaitAsync();
                _log.Log($"'{project.Id}' - Semaphore acquired.");

                try
                {
                    await ExecuteProjectAsync(project);
                }
                finally
                {
                    _semaphore.Release();
                    _log.Log($"'{project.Id}' - Semaphore released.");
                }
            });

            tasks.Add(task);
            _log.Log($"'{project.Id}' - Project finished.");
        }

        await Task.WhenAll(tasks);
    }

    private async Task ExecuteProjectAsync(ProjectParameters project)
    {
        _log.Log($"'{project.Id}' - Executing project.");
        var tasks = new List<Task>();

        for (int i = 0; i < project.TryCount; i++)
        {
            _log.Log($"'{project.Id}' - try '{i + 1}' started.");
            tasks.Add(Task.Run(() => ExecuteApp(project, i + 1)));
        }

        _log.Log($"'{project.Id}' - Waiting for all tries to finish.");
        await Task.WhenAll(tasks);
        _log.Log($"'{project.Id}' - Project executed.");
    }

    private void ExecuteApp(ProjectParameters project, int instanceNumber)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _appPath,
            Arguments = $"--app-timeout {project.AppTimeout} --memory-count {project.MemoryCount}",
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        var memoryIsEnough = _availableMemoryBytes.RawValue > _memoryThresholdBytes;

        try
        {
            _log.Log($"'{project.Id}' [{instanceNumber}] - Trying to execute app.");

            while (!memoryIsEnough)
            {
                _log.Log($"'{project.Id}' [{instanceNumber}] - Memory is not enough, waiting.");
                Thread.Sleep(DELAY_BETWEEN_MEMORY_CHECKS);

                memoryIsEnough = _availableMemoryBytes.RawValue > _memoryThresholdBytes;
            }

            _log.Log($"'{project.Id}' [{instanceNumber}] - Memory is enough, executing app.");
            process.Start();
            process.WaitForExit();
            _log.Log($"'{project.Id}' [{instanceNumber}] - App executed successfully.");
        }
        catch (Exception ex)
        {
            _log.Log($"'{project.Id}' [{instanceNumber}] - Failed to execute project. Exception: '{ex}'");
        }
    }
}
