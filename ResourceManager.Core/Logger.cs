namespace ResourceManager.Core;

public class Logger<T>
{
    private readonly string _filePath;
    private readonly string _className;
    private readonly object _lock = new();

    public bool IsEnabled { get; set; } = true;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Logger(string filePath)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        if (!IsEnabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                $"'{nameof(filePath)}' is not a valid path.",
                nameof(filePath));
        }

        _className = typeof(T).Name;
        _filePath = $"{_className}.{filePath}";

        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Dispose();
        }
        else
        {
            File.WriteAllText(_filePath, string.Empty);
        }

        var consoleAppLogPath = "../../../../ConsoleApp.log";

        if (File.Exists(consoleAppLogPath))
        {
            File.WriteAllText(consoleAppLogPath, string.Empty);
        }
    }

    public void Log(string message)
    {
        if (!IsEnabled)
        {
            return;
        }

        try
        {
            lock (_lock)
            {
                using StreamWriter writer = new(_filePath, true);

                writer.WriteLine($"[{DateTime.Now}] - {_className}: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}
