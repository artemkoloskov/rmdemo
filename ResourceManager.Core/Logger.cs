namespace ResourceManager.Core;

public class Logger<T>
{
    private readonly string _filePath;
    private readonly string _className;

    public Logger(string filePath)
    {
#if DEBUG
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
#endif
    }

    public void Log(string message)
    {
#if DEBUG
        try
        {
            using StreamWriter writer = new(_filePath, true);

            writer.WriteLine($"[{DateTime.Now}] - {_className}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
#endif
    }
}
