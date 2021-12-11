using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelliHome.Common.Tests;

[TestClass]
public sealed class EnsureTests
{
    [TestMethod]
    [DataRow(null, true)]
    [DataRow("Not Null", false)]
    public void TestEnsureNotNullDetectsNull(object? obj, bool isNull)
    {
        if (isNull)
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => Ensure.NotNull(obj));
            Assert.AreEqual(nameof(obj), exception.ParamName);
        }
        else
        {
            Ensure.NotNull(obj);
        }
    }

    [TestMethod]
    public void TestEnsureNotNullReturnsRightValue()
    {
        Assert.AreEqual("string", Ensure.NotNull("string"));
    }

    [TestMethod]
    [DataRow("text", false)]
    [DataRow(null, true)]
    [DataRow("", true)]
    [DataRow("  ", true)]
    [DataRow(" ", true)]
    [DataRow("\t", true)]
    [DataRow(" \t", true)]
    [DataRow("\t ", true)]
    [DataRow("\t\t", true)]
    public void TestEnsureNotNullOrWhiteSpaceDetectsNullOrWhiteSpace(string? str, bool isNullOrWhiteSpace)
    {
        if (isNullOrWhiteSpace)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => Ensure.NotNullOrWhiteSpace(str));
            Assert.AreEqual(nameof(str), exception.ParamName);
        }
        else
        {
            Ensure.NotNullOrWhiteSpace(str);
        }
    }

    [TestMethod]
    public void TestEnsureNotNullOrWhiteSpaceReturnsRightValue()
    {
        Assert.AreEqual("string", Ensure.NotNullOrWhiteSpace("string"));
    }

    [TestMethod]
    [DataRow(new object[] {1, 2, 3}, false)]
    [DataRow(null, true)]
    [DataRow(new object[] { }, true)]
    public void TestEnsureNotNullOrEmptyDetectsNullOrEmpty(object[]? arr, bool isNullOrEmpty)
    {
        if (isNullOrEmpty)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => Ensure.NotNullOrEmpty(arr));
            Assert.AreEqual(nameof(arr), exception.ParamName);
        }
        else
        {
            Ensure.NotNullOrEmpty(arr);
        }
    }

    [TestMethod]
    public void TestEnsureNotNullOrEmptyReturnsRightValue()
    {
        var arr = new[] {1, 2, 3};
        CollectionAssert.AreEqual(arr, Ensure.NotNullOrEmpty(arr).ToArray());
    }
}