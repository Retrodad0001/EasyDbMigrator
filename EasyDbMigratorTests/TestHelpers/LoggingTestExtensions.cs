﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;


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
            throw new ArgumentNullException(nameof(mockedLogger), "there is something wrong with your test!");
        }

        if (string.IsNullOrEmpty(expectedMessage))
        {
            throw new ArgumentException(
                new StringBuilder().Append('\'')
                    .Append(nameof(expectedMessage))
                    .Append("' cannot be null or empty, there is something wrong with your test!")
                    .ToString(), nameof(expectedMessage));
        }

        Func<object, Type, bool> state = (v, _) => string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0;

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
