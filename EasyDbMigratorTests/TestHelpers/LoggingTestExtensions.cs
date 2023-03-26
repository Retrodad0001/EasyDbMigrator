using Microsoft.Extensions.Logging;
using System;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class LoggingTestExtensions
{
    public static Mock<ILogger<T>> CheckIfLoggerWasCalled<T>(this Mock<ILogger<T>> mockedLogger
        , string expectedMessage
        , LogLevel expectedLogLevel
        , Times times
        , bool checkExceptionNotNull)
    {
        if (mockedLogger is null)
        {
            throw new ArgumentNullException(paramName: nameof(mockedLogger), message: "there is something wrong with your test!");
        }

        if (string.IsNullOrEmpty(value: expectedMessage))
        {
            throw new ArgumentException(message: $"'{nameof(expectedMessage)}' cannot be null or empty, there is something wrong with your test!", paramName: nameof(expectedMessage));
        }

        Func<object, Type, bool> state = (v, _) => string.Compare(strA: v.ToString(), strB: expectedMessage, comparisonType: StringComparison.Ordinal) == 0;

        if (checkExceptionNotNull)
        {
            mockedLogger.Verify(expression: x => x.Log(
                  It.Is<LogLevel>(l => l == expectedLogLevel),
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((v, t) => state(v, t)),
                  It.IsNotNull<Exception>(),
                  It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times: times);
        }
        else
        {

            mockedLogger.Verify(expression: x => x.Log(
                    It.Is<LogLevel>(l => l == expectedLogLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times: times);
        }

        return mockedLogger;
    }
}
