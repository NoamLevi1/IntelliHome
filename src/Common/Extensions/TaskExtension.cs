namespace IntelliHome.Common;

public static class TaskExtension
{
    public static Task WhenAllAsync(this IEnumerable<Task> tasks)
    {
        Ensure.NotNullOrEmpty(tasks);

        return Task.WhenAll(tasks);
    }

    public static async Task<TResult> WithTimeoutAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        var waitTask = Task.Run(
            async () =>
            {
                await Task.Delay(timeout);
                return default(TResult);
            });

        var finishedTask = await Task.WhenAny(task, waitTask!);

        if (finishedTask == waitTask)
        {
            throw new TimeoutException($"Task did not finished in allocated time [{nameof(timeout)}={timeout}]");
        }

        return task.Result;
    }
}