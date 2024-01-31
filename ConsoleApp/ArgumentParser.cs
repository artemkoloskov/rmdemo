namespace ConsoleApp;

internal static class ArgumentParser
{
    private const string _appTimeoutKey = "--app-timeout";
    private const string _memoryCountKey = "--memory-count";
    private const string _intanceIdKey = "--instance-id";
    private const string _logKey = "--log";
    private const int _appTimeoutDefault = 200;
    private const int _memoryCountDefault = 10;

    public static int ParseAppTimeout(string[] args)
    {
        var keyIndex = Array.IndexOf(args, _appTimeoutKey);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{_appTimeoutKey}' argument not found, using " +
                $"default value - {_appTimeoutDefault} milliseconds");
            return _appTimeoutDefault;
        }

        var value = args[keyIndex + 1];

        if (int.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{_appTimeoutKey}', using default value - {_appTimeoutDefault} milliseconds");
        return _appTimeoutDefault;
    }

    public static int ParseMemoryCount(string[] args)
    {
        var keyIndex = Array.IndexOf(args, _memoryCountKey);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{_memoryCountKey}' argument not found, using " +
                $"default value - {_memoryCountDefault} MB");
            return _memoryCountDefault;
        }

        var value = args[keyIndex + 1];

        if (int.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{_memoryCountKey}', using default value - {_memoryCountDefault} MB");
        return _memoryCountDefault;
    }

    public static string ParseInstanceId(string[] args)
    {
        var keyIndex = Array.IndexOf(args, _intanceIdKey);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{_intanceIdKey}' argument not found, using " +
                $"default value - '0_0'");
            return "0_0";
        }

        var value = args[keyIndex + 1];

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        Console.WriteLine($"'{value}' is empty or whitespace, using default value - '0_0'");
        return "0_0";
    }

    public static bool ParseLog(string[] args)
    {
        var keyIndex = Array.IndexOf(args, _logKey);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{_logKey}' argument not found, using " +
                $"default value - 'false'");
            return false;
        }

        var value = args[keyIndex + 1];

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{_logKey}', using default value - 'false'");
        return false;
    }

    public static void CheckArgs(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided, using default values");
            Console.WriteLine();

            PrintHelp();

            return;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("===================================================");
        Console.WriteLine("Usage:");
        Console.WriteLine($"    {_appTimeoutKey} <milliseconds>");
        Console.WriteLine($"    {_memoryCountKey} <MB>");
        Console.WriteLine($"    {_intanceIdKey} <instance id>");
        Console.WriteLine($"    {_logKey} <true/false>");
        Console.WriteLine("===================================================");
        Console.WriteLine();
    }
}
