#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
[TestClass]
public class ResultTests
{
    [ExcludeFromCodeCoverage]
    private sealed class MyCustomTypeUsedInResult { }

    [TestMethod]
    public void CanCreateSuccessResultWithCustomType()
    {
        Result<MyCustomTypeUsedInResult> result = new(true, new MyCustomTypeUsedInResult());

        Assert.IsTrue(result.WasSuccessful);
        Assert.IsFalse(result.HasFailure);
        Assert.IsNotNull(result.Value);
        Assert.IsNull(result.Exception);
    }

    [TestMethod]
    public void WhenCreatingFailureResultAnExceptionCanBeAdded()
    {
        Result<MyCustomTypeUsedInResult> result = new(false, new System.Exception());

        Assert.IsFalse(result.WasSuccessful);
        Assert.IsTrue(result.HasFailure);
        Assert.IsNull(result.Value);
        Assert.IsNotNull(result.Exception);
    }

    [TestMethod]
    public void WhenCreatingFailureResultExceptionIsNotMandatory()
    {
        Result<MyCustomTypeUsedInResult> result = new(false);

        Assert.IsFalse(result.WasSuccessful);
        Assert.IsTrue(result.HasFailure);
        Assert.IsNull(result.Value);
        Assert.IsNull(result.Exception);
    }
}
