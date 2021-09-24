using EasyDbMigrator;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;


namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class ResultTests
    {
        [ExcludeFromCodeCoverage]
        private class MyCustomTypeUsedInResult
        {
        }

        [Fact]
        public void can_create_succes_result_with_custom_type()
        {
            Result<MyCustomTypeUsedInResult> result = new Result<MyCustomTypeUsedInResult>(isSucces: true, new MyCustomTypeUsedInResult());

            _ = result.IsSuccess.Should().BeTrue();
            _ = result.IsFailure.Should().BeFalse();
            _ = result.Value.Should().NotBeNull();
            _ = result.Exception.Should().BeNull();
        }

        [Fact]
        public void when_creating_failure_result_an_Exception_can_be_added()
        {
            Result<MyCustomTypeUsedInResult> result = new Result<MyCustomTypeUsedInResult>(isSucces: false, new System.Exception());

            _ = result.IsSuccess.Should().BeFalse();
            _ = result.IsFailure.Should().BeTrue();
            _ = result.Value.Should().BeNull();
            _ = result.Exception.Should().NotBeNull();
        }

        [Fact]
        public void when_creating_failure_result_exception_is_not_mandatory()
        {
            Result<MyCustomTypeUsedInResult> result = new Result<MyCustomTypeUsedInResult>(isSucces: false);

            _ = result.IsSuccess.Should().BeFalse();
            _ = result.IsFailure.Should().BeTrue();
            _ = result.Value.Should().BeNull();
            _ = result.Exception.Should().BeNull();
        }
    }
}
