using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelliHome.Common.Tests;

[TestClass]
public sealed class StringExtensionTests
{
    [TestMethod]
    [DataRow("text", false)]
    [DataRow(null, true)]
    [DataRow("", true)]
    [DataRow("  ", true)]
    [DataRow(" ", true)]
    [DataRow("\t",true)]
    [DataRow(" \t",true)]
    [DataRow("\t ",true)]
    [DataRow("\t\t",true)]
    public void TestIsNullOrWhiteSpace(string? value, bool expectedResult)
    {
        Assert.AreEqual(expectedResult, value.IsNullOrWhiteSpace());
    }
}