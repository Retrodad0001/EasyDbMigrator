#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EasyDbMigrator;

//TODO when new union types are available in .net than use that instead of this custom Result type
public sealed class Result<T>
{
    public bool HasFailure => !WasSuccessful;
    public bool WasSuccessful { get; }
    public Exception? Exception { get; }
    public T? Value { get; }

    public Result(bool wasSuccessful, Exception? exception = null)
    {
        WasSuccessful = wasSuccessful;
        Value = default;
        Exception = exception;
    }

    public Result(bool wasSuccessful, T? value, Exception? exception = null)
    {
        WasSuccessful = wasSuccessful;
        Value = value;
        Exception = exception;
    }

    [ExcludeFromCodeCoverage] //this method is used for debug only
    public override string ToString()
    {
        return new StringBuilder().Append("isSuccess: ")
            .Append(WasSuccessful)
            .Append(" exception: ")
            .Append(Exception)
            .ToString();
    }
}