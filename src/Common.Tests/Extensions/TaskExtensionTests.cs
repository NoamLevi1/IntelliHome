using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelliHome.Common.Tests;

[TestClass]
public sealed class TaskExtensionTests
{
    [TestMethod]
    public async Task TestWhenAllAsyncAsync()
    {
        var tasks =
            new[]
            {
                Task.CompletedTask,
                Task.Delay(50),
            };

        await tasks.WhenAllAsync();

        Assert.IsTrue(tasks.All(task => task.IsCompleted));
    }
}