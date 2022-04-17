namespace IntelliHome.Common;

public static class TaskExtension
{
    public static Task WhenAllAsync(this IEnumerable<Task> tasks)
    {
        Ensure.NotNullOrEmpty(tasks);

        return Task.WhenAll(tasks);
    }

    public static void Await(this Task task) =>
        task.GetAwaiter().GetResult();

    public static TResult Await<TResult>(this Task<TResult> task) =>
        task.GetAwaiter().GetResult();

    public static void Await(this ValueTask task) =>
        task.GetAwaiter().GetResult();

    public static TResult Await<TResult>(this ValueTask<TResult> task) =>
        task.GetAwaiter().GetResult();
}