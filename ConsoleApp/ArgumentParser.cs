namespace ConsoleApp;

internal static class ArgumentParser
{
    private const string _appTimeoutKey = "--app-timeout";
    private const string _memoryCountKey = "--memory-count";
    private const int _appTimeoutDefault = 1;
    private const int _memoryCountDefault = 10;

    public static int ParseAppTimeout(string[] args)
    {
        var keyIndex = Array.IndexOf(args, _appTimeoutKey);

        if (keyIndex == -1)
        {
            Console.WriteLine($"'{_appTimeoutKey}' argument not found, using " +
                $"default value - {_appTimeoutDefault} seconds");
            return _appTimeoutDefault;
        }

        var value = args[keyIndex + 1];

        if (int.TryParse(value, out var result))
        {
            return result;
        }

        Console.WriteLine($"'{value}' is not a valid value for " +
            $"'{_appTimeoutKey}', using default value - {_appTimeoutDefault} seconds");
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
        Console.WriteLine($"    {_appTimeoutKey} <seconds>");
        Console.WriteLine($"    {_memoryCountKey} <MB>");
        Console.WriteLine("===================================================");
        Console.WriteLine();
    }
}
