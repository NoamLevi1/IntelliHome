using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public sealed class ClientStoreTests
{
    private HomeApplianceStore _store = null!;

    [TestMethod]
    public void TestCanGetClientIds()
    {
        _ = _store.ConnectedHomeApplianceIds;
    }

    [TestMethod]
    public void TestAddClient()
    {
        _store.AddOrUpdateHomeAppliance(Guid.Empty, "connectionId");

        Assert.AreEqual("connectionId", _store.GetConnectionId(Guid.Empty));
    }

    [TestMethod]
    public void TestAddClientCallsEvent()
    {
        var isUpdated = false;

        _store.ConnectedHomeAppliancesChanged += () => isUpdated = true;

        _store.AddOrUpdateHomeAppliance(Guid.Empty, string.Empty);

        Assert.AreEqual(true, isUpdated);
    }

    [TestInitialize]
    public void Initialize()
    {
        _store = new HomeApplianceStore();
    }
}