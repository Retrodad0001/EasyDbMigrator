using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator
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

        public Result(bool isSucces, T value, Exception? exception = null)
        {
            IsSuccess = isSucces;
            Value = value;
            Exception = exception;
        }

        [ExcludeFromCodeCoverage] //this method is used for debug only
        public override string ToString()
        {
            return $"isSucces: {IsSuccess} exception: {Exception}";
        }
    }

}




