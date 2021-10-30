using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator
{
    public struct Result<T>
    {
        public bool HasFailure => !WasSuccessful;
        public bool WasSuccessful { get; private set; }
        public Exception? Exception { get; private set; }
        public T Value { get; private set; }//TODO change T to nullable field when .net 3.1 not supported, remove warnings

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Result(bool wasSuccessful, Exception? exception = null)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            WasSuccessful = wasSuccessful;
#pragma warning disable CS8601 // Possible null reference assignment.
            Value = default(T);
#pragma warning restore CS8601 // Possible null reference assignment.
            Exception = exception;
        }

        public Result(bool wasSuccessful, T value, Exception? exception = null)//TODO change T to nullable field when .net 3.1 not supported
        {
            WasSuccessful = wasSuccessful;
            Value = value;
            Exception = exception;
        }

        [ExcludeFromCodeCoverage] //this method is used for debug only
        public override string ToString()
        {
            return $"isSucces: {WasSuccessful} exception: {Exception}";
        }
    }
}