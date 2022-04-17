using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public sealed class ClientStoreTests
{
    [TestMethod]
    public void TestCanGetClients()
    {
        _ = new ClientStore().Clients;
    }
}