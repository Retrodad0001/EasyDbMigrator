#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
public class ResultTests
{
    [ExcludeFromCodeCoverage]
    private sealed class MyCustomTypeUsedInResult { }

    [Fact]
    public void CanCreateSuccessResultWithCustomType()
    {
        Result<MyCustomTypeUsedInResult> result = new(true, new MyCustomTypeUsedInResult());

        Assert.True(result.WasSuccessful);
        Assert.False(result.HasFailure);
        Assert.NotNull(result.Value);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void WhenCreatingFailureResultAnExceptionCanBeAdded()
    {
        Result<MyCustomTypeUsedInResult> result = new(false, new System.Exception());

        Assert.False(result.WasSuccessful);
        Assert.True(result.HasFailure);
        Assert.Null(result.Value);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public void WhenCreatingFailureResultExceptionIsNotMandatory()
    {
        Result<MyCustomTypeUsedInResult> result = new(false);

        Assert.False(result.WasSuccessful);
        Assert.True(result.HasFailure);
        Assert.Null(result.Value);
        Assert.Null(result.Exception);
    }
}
