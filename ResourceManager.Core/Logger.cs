namespace ResourceManager.Core;

/// <summary>
/// A generic logger class that logs messages to a file.
/// </summary>
/// <typeparam name="T">The type of the class that is using the logger. It
/// will define the name of the log file.
/// </typeparam>
public class Logger<T>
{
    private readonly string _filePath = "";
    private readonly string _className = "";
    private readonly object _lock = new();

    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger{T}"/> class.
    /// This will create a new log file or clear the existing one. The file
    /// name provided will be prefixed with the name of the class that is
    /// using the logger.
    /// </summary>
    /// <param name="fileName"></param>
    /// <exception cref="ArgumentException"></exception>
    public Logger(string fileName)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (fileName is null or "")
        {
            throw new ArgumentException(
                $"'{nameof(fileName)}' is not a valid path.",
                nameof(fileName));
        }

        _className = typeof(T).Name;
        _filePath = $"{_className}.{fileName}";

        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Dispose();
        }
        else
        {
            File.WriteAllText(_filePath, string.Empty);
        }
    }

    /// <summary>
    /// Logs a message to the log file.
    /// </summary>
    /// <param name="message"></param>
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
