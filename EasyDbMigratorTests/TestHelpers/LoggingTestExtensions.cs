using Microsoft.Extensions.Logging;
using System;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.TestHelpers
{
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
                throw new ArgumentNullException(nameof(mockedLogger), "there is something wrong with your test!");
            }

            if (string.IsNullOrEmpty(expectedMessage))
            {
                throw new ArgumentException($"'{nameof(expectedMessage)}' cannot be null or empty, there is something wrong with your test!", nameof(expectedMessage));
            }

#pragma warning disable CA1310 // Specify StringComparison for correctness
            Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;
#pragma warning restore CA1310 // Specify StringComparison for correctness

            if (checkExceptionNotNull)
            {
                mockedLogger.Verify(x => x.Log(
                      It.Is<LogLevel>(l => l == expectedLogLevel),
                      It.IsAny<EventId>(),
                      It.Is<It.IsAnyType>((v, t) => state(v, t)),
                      It.IsNotNull<Exception>(),
                      It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);
            }
            else
            {

                mockedLogger.Verify(x => x.Log(
                        It.Is<LogLevel>(l => l == expectedLogLevel),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => state(v, t)),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);
            }

            return mockedLogger;
        }
    }
}
