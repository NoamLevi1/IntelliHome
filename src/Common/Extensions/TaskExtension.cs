namespace IntelliHome.Common;

public static class TaskExtension
{
    public static Task WhenAllAsync(this IEnumerable<Task> tasks)
    {
        Ensure.NotNullOrEmpty<IEnumerable<Task>, Task>(tasks);

        return Task.WhenAll(tasks);
    }
}