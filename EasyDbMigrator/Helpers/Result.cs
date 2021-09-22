using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator.Helpers
{
    public struct Result<T>
    {
        public bool IsFailure => !IsSuccess;
        public bool IsSuccess { get; private set; }

        public Exception? Exception { get; private set; }

        public T? Value { get; private set; }

        public Result(bool isSucces, Exception? exception = null)
        {
            IsSuccess = isSucces;
            Value = default(T);
            Exception = exception;
        }

        public Result(bool isSucces, T value)
        {
            IsSuccess = isSucces;
            Value = value;
            Exception = null;
        }

        [ExcludeFromCodeCoverage] //this method is used for debug only
        public override string ToString()
        {
            return $"isSucces: {IsSuccess} exception: {Exception}";
        }
    }
}




