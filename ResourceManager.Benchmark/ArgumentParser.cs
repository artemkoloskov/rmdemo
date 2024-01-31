namespace ResourceManager.Benchmark;

public static class ArgumentParser
{
    private const string APP_PATH_KEY = "--app-path";
    private const string PROJECTS_PARAMETERS_PATH_KEY = "--json-path";
    private const string MAX_GLOBAL_THREADS_KEY = "--max-global-threads";
    private const string MEMORY_THRESHOLD_MB_KEY = "--memory-threshold-mb";
    private const string PROCESSOR_TIME_THRESHOLD_KEY = "--processor-time-threshold";
    private const string ENABLE_LOGGING_KEY = "--log";
    private const string REPORTS_PATH_KEY = "--reports-path";

    private const string DEFAULT_APP_PATH = "../../../../ConsoleApp/bin/Release/net8.0/ConsoleApp.exe";
    private const string DEFAULT_PROJECTS_PATH = "projects.json";
    private const int DEFAULT_MAX_GLOBAL_THREADS = 20;
    private const int DEFAULT_MEMORY_THRESHOLD_MB = 200;
    private const float DEFAULT_PROCESSOR_TIME_THRESHOLD = 5f;
    private const bool DEFAULT_ENABLE_LOGGING = false;
    private const string DEFAULT_REPORTS_PATH = "C:\\temp";

    public static void CheckArgs(string[] args)
    {
        PrintHelp();

        if (args.Length < 2
            || !args.Contains(APP_PATH_KEY))
        {
            Console.WriteLine("Missing arguments, using default values for " +
                "app path, projects path, max global threads, memory threshold " +
                "and logging");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("===================================================");
        Console.WriteLine("Usage:");
        Console.WriteLine($"    {APP_PATH_KEY} <path>              - optional, default value - " +
            $"'{DEFAULT_APP_PATH}', path to the console app that is being " +
            $"executed by the resource manager");
        Console.WriteLine($"    {PROJECTS_PARAMETERS_PATH_KEY} <path>             - optional, " +
            $"default value - '{DEFAULT_PROJECTS_PATH}', path to the json file " +
            $"containing the projects' parameters");
        Console.WriteLine($"    {MAX_GLOBAL_THREADS_KEY} <number>  - optional, " +
            $"default value - {DEFAULT_MAX_GLOBAL_THREADS}, maximum number of " +
            $"threads that can be used by the resource manager");
        Console.WriteLine($"    {MEMORY_THRESHOLD_MB_KEY} <number> - optional, " +
            $"default value - {DEFAULT_MEMORY_THRESHOLD_MB} MB, an amount of " +
            $"memory that should be free at all times while resource manager is " +
            $"running, in megabytes");
        Console.WriteLine($"    {ENABLE_LOGGING_KEY} <true/false>             - optional, " +
            $"default value - {DEFAULT_ENABLE_LOGGING}, whether to enable " +
            $"logging or not. Be aware, that logging can significantly slow down " +
            $"the execution of the resource manager. Also, BenchmarkDotNet " +
            $"will delete the log file after the benchmark is finished");
        Console.WriteLine($"    {PROCESSOR_TIME_THRESHOLD_KEY} <number> - optional, " +
            $"default value - {DEFAULT_PROCESSOR_TIME_THRESHOLD}, a threshold " +
            $"for the processor time, in percent, that should be free at all " +
            $"times while resource manager is running");
        Console.WriteLine($"    {REPORTS_PATH_KEY} <path>             - optional, " +
            $"default value - '{DEFAULT_REPORTS_PATH}', path to the folder " +
            $"where the reports will be saved");
        Console.WriteLine("===================================================");
        Console.WriteLine();
    }

    public static string ParseAppPath(string[] args)
    {
        var keyIndex = Array.IndexOf(args, APP_PATH_KEY);

        if (keyIndex == -1)
        {
            var fullPath = Path.GetFullPath(DEFAULT_APP_PATH);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Default app path {fullPath} not found");
            }

            Console.WriteLine($"'{APP_PATH_KEY}' argument not found, using " +
                $"default value - '{fullPath}'");

            return fullPath;
        }

        var value = args[keyIndex + 1];

        if (string.IsNullOrWhiteSpace(value)
            || !File.Exists(value)
            || !Path.GetExtension(value).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"'{value}' is not a valid path to an existing file");
        }

        return value;
    }

    public static string ParseProjectsParametersPath(string[] args)
    {
        var keyIndex = Array.IndexOf(args, PROJECTS_PARAMETERS_PATH_KEY);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{PROJECTS_PARAMETERS_PATH_KEY}' argument not found, using " +
                $"default value - '{DEFAULT_PROJECTS_PATH}'");
            return DEFAULT_PROJECTS_PATH;
        }

        var value = args[keyIndex + 1];

        if (string.IsNullOrWhiteSpace(value)
            || !File.Exists(value)
            || !Path.GetExtension(value).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"'{value}' is not a valid path to an existing file");
        }

        return value;
    }

    public static int ParseMaxGlobalThreads(string[] args)
    {
        var keyIndex = Array.IndexOf(args, MAX_GLOBAL_THREADS_KEY);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{MAX_GLOBAL_THREADS_KEY}' argument not found, using " +
                $"default value - '{DEFAULT_MAX_GLOBAL_THREADS}'");
            return DEFAULT_MAX_GLOBAL_THREADS;
        }

        var value = args[keyIndex + 1];

        if (int.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{MAX_GLOBAL_THREADS_KEY}', using default value - '1'");
        return DEFAULT_MAX_GLOBAL_THREADS;
    }

    public static int ParseMemoryThresholdMb(string[] args)
    {
        var keyIndex = Array.IndexOf(args, MEMORY_THRESHOLD_MB_KEY);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{MEMORY_THRESHOLD_MB_KEY}' argument not found, using " +
                $"default value - '{DEFAULT_MEMORY_THRESHOLD_MB}'");
            return DEFAULT_MEMORY_THRESHOLD_MB;
        }

        var value = args[keyIndex + 1];

        if (int.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{MEMORY_THRESHOLD_MB_KEY}', using default value - '{DEFAULT_MEMORY_THRESHOLD_MB}'");
        return DEFAULT_MEMORY_THRESHOLD_MB;
    }

    public static bool ParseEnableLogging(string[] args)
    {
        var keyIndex = Array.IndexOf(args, ENABLE_LOGGING_KEY);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{ENABLE_LOGGING_KEY}' argument not found, using " +
                $"default value - '{DEFAULT_ENABLE_LOGGING}'");
            return DEFAULT_ENABLE_LOGGING;
        }

        var value = args[keyIndex + 1];

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{ENABLE_LOGGING_KEY}', using default value - '{DEFAULT_ENABLE_LOGGING}'");
        return DEFAULT_ENABLE_LOGGING;
    }

    public static float ParseProcessorTimeThreshold(string[] args)
    {
        var keyIndex = Array.IndexOf(args, PROCESSOR_TIME_THRESHOLD_KEY);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{PROCESSOR_TIME_THRESHOLD_KEY}' argument not found, using " +
                $"default value - '{DEFAULT_PROCESSOR_TIME_THRESHOLD}'");
            return DEFAULT_PROCESSOR_TIME_THRESHOLD;
        }

        var value = args[keyIndex + 1];

        if (float.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{PROCESSOR_TIME_THRESHOLD_KEY}', using default value - " +
            $"'{DEFAULT_PROCESSOR_TIME_THRESHOLD}'");
        return DEFAULT_PROCESSOR_TIME_THRESHOLD;
    }

    public static string ParseReportsPath(string[] args)
    {
        var keyIndex = Array.IndexOf(args, REPORTS_PATH_KEY);

        if (keyIndex == -1)
        {
            var fullPath = Path.GetFullPath(DEFAULT_REPORTS_PATH);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"Default reports path {fullPath} not found");
            }

            Console.WriteLine($"'{REPORTS_PATH_KEY}' argument not found, using " +
                $"default value - '{fullPath}'");

            return fullPath;
        }

        var value = args[keyIndex + 1];

        if (string.IsNullOrWhiteSpace(value)
            || !Directory.Exists(value))
        {
            throw new ArgumentException($"'{value}' is not a valid path to an existing directory");
        }

        return value;
    }
}
