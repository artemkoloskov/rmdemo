using ConsoleApp;

ArgumentParser.CheckArgs(args);

int _appTimeout = ArgumentParser.ParseAppTimeout(args);
int _memoryCount = ArgumentParser.ParseMemoryCount(args);

var cancellationToken = new CancellationTokenSource();
var _timer = new Timer(
    _ => cancellationToken.Cancel(), 
    null, 
    TimeSpan.FromMilliseconds(_appTimeout),
    Timeout.InfiniteTimeSpan);

var memory = new byte[_memoryCount * 1024 * 1024];

var counter = 0;

while (!cancellationToken.IsCancellationRequested)
{
    counter++;
}
