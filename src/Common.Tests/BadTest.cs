using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelliHome.Common.Tests;

[TestClass]
public class BadTest
{
    [TestMethod]
    public void Fail()
    {
        Assert.AreEqual(0,1);
    }
}