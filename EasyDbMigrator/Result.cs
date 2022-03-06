using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator
{
    public struct Result<T>
    {
        public bool HasFailure => !WasSuccessful;
        public bool WasSuccessful { get; }
        public Exception? Exception { get; }
        public T? Value { get; }

        public Result(bool wasSuccessful, Exception? exception = null)
        {
            WasSuccessful = wasSuccessful;
            Value = default(T);
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
            return $"isSuccess: {WasSuccessful} exception: {Exception}";
        }
    }
}