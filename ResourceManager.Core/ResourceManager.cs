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
    private readonly SemaphoreSlim _globalSemaphore;
    private readonly PerformanceCounter _availableMemoryBytes =
        new("Memory", "Available Bytes");
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
            $"max global threads '{maxGlobalThreads}' and memory threshold " +
            $"'{memoryThresholdBytes}'.");

        ArgumentOutOfRangeException.ThrowIfLessThan(maxGlobalThreads, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(memoryThresholdBytes, MIN_MEMORY_THRESHOLD_BYTES);

        _globalSemaphore = new SemaphoreSlim(maxGlobalThreads);
        _memoryThresholdBytes = memoryThresholdBytes;
        _appPath = ValidateAppPath(appPath);
        _projects = ParseProjects(projectsParametersPath);

        _log.Log($"Resource manager initialized successfully. Number of " +
            $"projects loaded: {_projects.Count}.");
    }

    public void EnableLogging()
    {
        _log.IsEnabled = true;
    }

    public void ExecuteProjects()
    {
        _log.Log($"Setting up projects.");

        List<Task> tasks = GetProjectsTaskList();

        _log.Log($"Waiting for all projects to finish.");

        Task.WaitAll([.. tasks]);
    }

    private List<Task> GetProjectsTaskList()
    {
        var tasks = new List<Task>();

        foreach (var project in _projects)
        {
            _log.Log($"[{project.Id}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"Memory count '{project.MemoryCount}', " +
                $"app timeout '{project.AppTimeout}', try count '{project.TryCount}', " +
                $"max threads '{project.MaxThreads}'.");

            tasks.Add(Task.Run(() => ExecuteProject(project)));

            _log.Log($"[{project.Id}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"Project set up.");
        }

        return tasks;
    }

    private void ExecuteProject(ProjectParameters project)
    {
        _log.Log($"[{project.Id}]" +
            $"[Thread:{Environment.CurrentManagedThreadId}] - " +
            $"Executing project.");

        var tasks = GetProjectTriesTaskList(project);

        _log.Log($"[{project.Id}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"Waiting for all tries to finish.");

        Task.WaitAll([.. tasks]);

        _log.Log($"[{project.Id}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"Project executed.");
    }

    private List<Task> GetProjectTriesTaskList(ProjectParameters project)
    {
        var projectSemaphore = new SemaphoreSlim(project.MaxThreads ?? 1);

        var tasks = new List<Task>();

        for (int i = 0; i < project.TryCount; i++)
        {
            var tryId = i + 1;

            _log.Log($"[{project.Id}_{tryId}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"instance initiated.");

            tasks.Add(Task.Run(() => ExecuteApp(project, tryId, projectSemaphore)));
        }

        return tasks;
    }

    private void ExecuteApp(ProjectParameters project, int instanceIndex, SemaphoreSlim projectSemaphore)
    {
        using Process process = CreateNewProcess(project, instanceIndex);

        ReserveThread(projectSemaphore, $"{project.Id}_{instanceIndex}");

        try
        {
            _log.Log($"[{project.Id}_{instanceIndex}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"Trying to execute app.");

            WaitForEnoughMemory(project, instanceIndex);

            process.Start();

            _log.Log($"[{project.Id}_{instanceIndex}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"App execution started.");

            process.WaitForExit();

            _log.Log($"[{project.Id}_{instanceIndex}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"App execution finished. Exit code: {process.ExitCode}.");
        }
        catch (Exception ex)
        {
            _log.Log($"[{project.Id}_{instanceIndex}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"Failed to execute application. Exception: '{ex}'");
        }
        finally
        {
            ReleaseThread(projectSemaphore, $"{project.Id}_{instanceIndex}");
        }
    }

    private void ReleaseThread(SemaphoreSlim projectSemaphore, string instanceId)
    {
        var projectCount = projectSemaphore.Release();
        var globalCount = _globalSemaphore.Release();

        _log.Log($"[{instanceId}]" +
            $"[Thread:{Environment.CurrentManagedThreadId}] - " +
            $"Semaphores are released. Global semaphore count: {globalCount}, " +
            $"project semaphore count: {projectCount}.");
    }

    private void ReserveThread(SemaphoreSlim projectSemaphore, string instanceId)
    {
        _globalSemaphore.Wait();
        projectSemaphore.Wait();

        _log.Log($"[{instanceId}]" +
            $"[Thread:{Environment.CurrentManagedThreadId}] - " +
            $"Semaphores are acquired.");
    }

    private void WaitForEnoughMemory(ProjectParameters project, int instanceIndex)
    {
        _log.Log($"[{project.Id}_{instanceIndex}]" +
            $"[Thread:{Environment.CurrentManagedThreadId}] - " +
            $"Memory left: {_availableMemoryBytes.RawValue / 1024 / 1024} MB");

        var notEnoughMemory = _availableMemoryBytes.RawValue < _memoryThresholdBytes + project.MemoryCount * 1024 * 1024;

        while (notEnoughMemory)
        {
            _log.Log($"[{project.Id}_{instanceIndex}]" +
                $"[Thread:{Environment.CurrentManagedThreadId}] - " +
                $"Not enough memory, waiting.");

            Thread.Sleep(DELAY_BETWEEN_MEMORY_CHECKS);

            notEnoughMemory = _availableMemoryBytes.RawValue < _memoryThresholdBytes + project.MemoryCount * 1024 * 1024;
        }

        _log.Log($"[{project.Id}_{instanceIndex}]" +
            $"[Thread:{Environment.CurrentManagedThreadId}] - " +
            $"There are enough memory.");
    }

    private Process CreateNewProcess(ProjectParameters project, int instanceIndex)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _appPath,
            Arguments = $"--app-timeout {project.AppTimeout} --memory-count {project.MemoryCount} --instance-id {project.Id}_{instanceIndex} --log {_log.IsEnabled}",
            CreateNoWindow = true,
            WorkingDirectory = "../../../../",
        };

        var process = new Process { StartInfo = processStartInfo };

        return process;
    }

    private static List<ProjectParameters> ParseProjects(string projectsParametersPath)
    {
        if (string.IsNullOrWhiteSpace(projectsParametersPath)
            || !File.Exists(projectsParametersPath))
        {
            throw new ArgumentException(
                $"'{nameof(projectsParametersPath)}' is not a valid path to an existing file.",
                nameof(projectsParametersPath));
        }

        var json = File.ReadAllText(projectsParametersPath);

        return ProjectsList.FromJson(json)?.Projects
            ?? throw new Exception($"Failed to deserialize projects from " +
                $"'{projectsParametersPath}' file.");
    }

    private static string ValidateAppPath(string appPath)
    {
        return string.IsNullOrWhiteSpace(appPath)
                || !File.Exists(appPath)
                || !Path.GetExtension(appPath).Equals(".exe", StringComparison.OrdinalIgnoreCase)
            ? throw new ArgumentException(
                $"'{nameof(appPath)}' is not a valid path to an existing file.",
                nameof(appPath))
            : appPath;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _globalSemaphore.Dispose();
    }
}
