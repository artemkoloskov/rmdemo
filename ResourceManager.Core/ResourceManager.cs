using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace ResourceManager.Core;

/// <summary>
/// Manages the execution of multiple projects with multiple tries each.
/// Uses a semaphore to limit the number of global threads and a semaphore
/// for each project to limit the number of threads per project.
/// <see cref="ResourceMonitor"/> is used to monitor the resources, to
/// ensure that the system stays responsive.
/// <see cref="Logger{T}"/> is used for logging. In release mode, logging
/// is disabled by default.
/// After all projects are executed, a report is saved to a file, if
/// reporting is enabled.
/// </summary>
[SupportedOSPlatform("windows")]
public class ResourceManager : IDisposable
{
    private readonly string _appPath;
    private readonly List<ProjectParameters> _projects;
    private readonly SemaphoreSlim _globalSemaphore;
    private readonly ResourceMonitor _resourceMonitor;
    private readonly Logger<ResourceManager> _log = new("log")
    {
#if DEBUG
        IsEnabled = true
#else
        IsEnabled = false
#endif
    };

    private string _reportPath = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceManager"/> class.
    /// All parameters are validated and the projects are loaded from the
    /// specified file.
    /// </summary>
    /// <param name="appPath">Path to the application that will be executed
    /// by the resource manager.</param>
    /// <param name="projectsParametersPath">Path to the file containing
    /// the projects' parameters.</param>
    /// <param name="maxGlobalThreads">Maximum number of threads that can
    /// be used by the resource manager.</param>
    /// <param name="memoryThresholdBytes">An amount of memory that should
    /// be free at all times while resource manager is running, in bytes.</param>
    /// <param name="processorTimeThreshold">A threshold for the processor
    /// time, in percent, that should be free at all times while resource
    /// manager is running.</param>
    public ResourceManager(
        string appPath,
        string projectsParametersPath,
        int maxGlobalThreads,
        int memoryThresholdBytes,
        float processorTimeThreshold)
    {
        _log.Log($"Initializing resource manager with app path '{appPath}', " +
            $"projects parameters path '{projectsParametersPath}', " +
            $"max global threads '{maxGlobalThreads}', memory threshold " +
            $"'{memoryThresholdBytes}' and processor time threshold " +
            $"'{processorTimeThreshold}'");

        ArgumentOutOfRangeException.ThrowIfLessThan(maxGlobalThreads, 1);

        _resourceMonitor = new ResourceMonitor(memoryThresholdBytes, processorTimeThreshold);
        _globalSemaphore = new SemaphoreSlim(maxGlobalThreads);
        _appPath = ValidateAppPath(appPath);
        _projects = ProjectsList.ParseProjects(projectsParametersPath);

        _log.Log($"Resource manager initialized successfully. Number of " +
            $"projects loaded: {_projects.Count}.");
    }

    /// <summary>
    /// Enables logging. Logging will significantly slow down the execution
    /// of the resource manager.
    /// </summary>
    public void EnableLogging()
    {
        _log.IsEnabled = true;
    }

    /// <summary>
    /// Enables reporting. The report will be saved to the specified path
    /// after all projects are executed. Reports are saved in CSV format  with
    /// a name in the format 'report_yyyy_MM_dd-hh_mm_ss.csv'.
    /// </summary>
    /// <param name="reportPath">Path to the directory where the report
    /// will be saved.</param>
    /// <exception cref="ArgumentException"></exception>
    public void EnableReporting(string reportPath)
    {
        if (string.IsNullOrWhiteSpace(reportPath)
            || !Directory.Exists(Path.GetDirectoryName(reportPath)))
        {
            throw new ArgumentException(
                $"'{nameof(reportPath)}' is not a valid path to an existing directory.",
                nameof(reportPath));
        }

        _reportPath = reportPath;
    }

    /// <summary>
    /// Executes all projects loaded from the file. The projects are executed
    /// in parallel, with a semaphore to limit the number of global threads
    /// and a semaphore for each project to limit the number of threads per
    /// project. After all projects are executed, a report is saved to a file,
    /// if reporting is enabled.
    /// </summary>
    public void ExecuteProjects()
    {
        _log.Log($"Setting up projects.");

        List<Task> tasks = GetProjectsTaskList();

        _log.Log($"Waiting for all projects to finish.");

        Task.WaitAll([.. tasks]);

        _log.Log($"All projects finished.");

        SaveReport();
    }

    private void SaveReport()
    {
        if (string.IsNullOrWhiteSpace(_reportPath))
        {
            return;
        }

        var report = new StringBuilder();

        var totalExecutions = _projects.Sum(p => p.TryCount);
        var totalProjectedTime = _projects.Sum(p => p.TryCount * p.AppTimeout) / 1000;
        var totalActualTime = _resourceMonitor.TotalProcessingTime() / 1000;
        var totalCpuUsage = _resourceMonitor.AverageProcessorTimeInUse();
        var totalMemoryUsage = (int)_resourceMonitor.AverageMemoryInUseMb();

        _log.Log($"Saving report to '{_reportPath}'. Total executions: " +
            $"{totalExecutions}, total projected time: {totalProjectedTime}, " +
            $"total actual time: {totalActualTime}, total CPU usage: " +
            $"{totalCpuUsage}, total memory usage: {totalMemoryUsage}");

        report.AppendLine($"Projects number,{_projects.Count}");
        report.AppendLine($"Total executions,{totalExecutions}");
        report.AppendLine($"Projected sequential execution time (s),{totalProjectedTime}");
        report.AppendLine($"Actual execution time (s),{totalActualTime}");
        report.AppendLine($"Average CPU usage (%),{totalCpuUsage}");
        report.AppendLine($"Average memory in use (MB),{totalMemoryUsage}");
        report.AppendLine();

        var reportPath = Path.Combine(_reportPath, $"report_{DateTime.Now:yyyy_MM_dd-hh_mm_ss}.csv");

        File.Create(reportPath).Dispose();

        File.WriteAllText(reportPath, report.ToString());
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

            _resourceMonitor.WaitForEnoughResources(project.MemoryCount ?? 0);

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
