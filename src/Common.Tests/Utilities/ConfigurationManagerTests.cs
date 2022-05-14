using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace IntelliHome.Common.Tests;

[TestClass]
public sealed class ConfigurationManagerTests
{
    private readonly ConfigurationManager _manager;
    private readonly TestConfiguration _testConfiguration;

    public ConfigurationManagerTests()
    {
        _testConfiguration = new TestConfiguration
        {
            RightConfiguration = new TestRightConfiguration()
        };
        _manager = new ConfigurationManager(_testConfiguration);
    }

    [TestMethod]
    public void TestGetRightConfiguration() =>
        Assert.AreSame(
            _testConfiguration.RightConfiguration,
            _manager.Get<TestRightConfiguration>());

    [TestMethod]
    public void TestNotIncludedConfigurationThrowsException() =>
        Assert.ThrowsException<KeyNotFoundException>(
            () => _manager.Get<NonIncludedConfiguration>());

    private sealed class TestConfiguration : IServiceConfiguration
    {
        public TestRightConfiguration? RightConfiguration { get; init; }
    }

    private sealed class TestRightConfiguration : IServiceConfiguration
    {
    }

    [UsedImplicitly]
    private sealed class NonIncludedConfiguration : IServiceConfiguration
    {
    }
}