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
        Result<MyCustomTypeUsedInResult> result = new(wasSuccessful: true, value: new MyCustomTypeUsedInResult());

        _ = result.WasSuccessful.Should().BeTrue();
        _ = result.HasFailure.Should().BeFalse();
        _ = result.Value.Should().NotBeNull();
        _ = result.Exception.Should().BeNull();
    }

    [Fact]
    public void When_creating_failure_result_an_Exception_can_be_added()
    {
        Result<MyCustomTypeUsedInResult> result = new(wasSuccessful: false, exception: new System.Exception());

        _ = result.WasSuccessful.Should().BeFalse();
        _ = result.HasFailure.Should().BeTrue();
        _ = result.Value.Should().BeNull();
        _ = result.Exception.Should().NotBeNull();
    }

    [Fact]
    public void When_creating_failure_result_exception_is_not_mandatory()
    {
        Result<MyCustomTypeUsedInResult> result = new(wasSuccessful: false);

        _ = result.WasSuccessful.Should().BeFalse();
        _ = result.HasFailure.Should().BeTrue();
        _ = result.Value.Should().BeNull();
        _ = result.Exception.Should().BeNull();
    }
}
