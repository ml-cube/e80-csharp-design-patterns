using System.Diagnostics;
using System.Reflection;

namespace CSharpCourse.DesignPatterns.Assignments;

internal interface IService
{
    void DoSomething();
    Task DoSomethingAsync();
    Task<bool> GetResultAsync();
}

internal class SlowService : IService
{
    private readonly TimeSpan _delay;

    public SlowService(TimeSpan delay)
    {
        _delay = delay;
    }

    public void DoSomething()
    {
        Thread.Sleep(_delay);
    }

    public async Task DoSomethingAsync()
    {
        await Task.Delay(_delay);
    }

    public async Task<bool> GetResultAsync()
    {
        await Task.Delay(_delay);
        return true;
    }
}

internal class PerformanceMonitoringProxy<T>: DispatchProxy where T : class
{
    private T? _decorated;
    private TimeSpan _threshold;

    public static T Create(T decorated, TimeSpan threshold)
    {
        object proxy = Create<T, PerformanceMonitoringProxy<T>>();

        ((PerformanceMonitoringProxy<T>)proxy)._decorated = decorated;
        ((PerformanceMonitoringProxy<T>)proxy)._threshold = threshold;
        
        return (T)proxy;
    }

    //protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    //{
    //    ArgumentNullException.ThrowIfNull(targetMethod);

    //    var sw = Stopwatch.StartNew();
    //    var result = targetMethod.Invoke(_decorated, args);
    //    sw.Stop();

    //    if (sw.Elapsed > _threshold)
    //        Console.WriteLine($"Method {targetMethod.Name} took {sw.Elapsed}");

    //    return result;
    //}

    //protected override async Task<object?> Invoke(MethodInfo? targetMethod, object?[]? args)
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        var sw = Stopwatch.StartNew();
        var result = targetMethod.Invoke(_decorated, args);

        if (result is Task taskResult)
        {
            // await taskResult
            //sw.Stop();

            //if (sw.Elapsed > _threshold)
            //    Console.WriteLine($"Method {targetMethod.Name} took {sw.Elapsed}");

            return Task.Run(async () =>
            {
                await taskResult;
                sw.Stop();

                if (sw.Elapsed > _threshold)
                    Console.WriteLine($"Method {targetMethod.Name} took {sw.Elapsed}");
            });
        }
        else
        {
            sw.Stop();
            if (sw.Elapsed > _threshold)
                Console.WriteLine($"Method {targetMethod.Name} took {sw.Elapsed}");
        }

        return result;
    }
}
