#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
public class ResultTests
{
    [ExcludeFromCodeCoverage]
    private sealed class MyCustomTypeUsedInResult { }

    [Fact]
    public void Can_create_success_result_with_custom_type()
    {
        Result<MyCustomTypeUsedInResult> result = new(true, new MyCustomTypeUsedInResult());

        _ = result.WasSuccessful.Should().BeTrue();
        _ = result.HasFailure.Should().BeFalse();
        _ = result.Value.Should().NotBeNull();
        _ = result.Exception.Should().BeNull();
    }

    [Fact]
    public void When_creating_failure_result_an_Exception_can_be_added()
    {
        Result<MyCustomTypeUsedInResult> result = new(false, new System.Exception());

        _ = result.WasSuccessful.Should().BeFalse();
        _ = result.HasFailure.Should().BeTrue();
        _ = result.Value.Should().BeNull();
        _ = result.Exception.Should().NotBeNull();
    }

    [Fact]
    public void When_creating_failure_result_exception_is_not_mandatory()
    {
        Result<MyCustomTypeUsedInResult> result = new(false);

        _ = result.WasSuccessful.Should().BeFalse();
        _ = result.HasFailure.Should().BeTrue();
        _ = result.Value.Should().BeNull();
        _ = result.Exception.Should().BeNull();
    }
}
