﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator
{
    public struct Result<T>
    {
        public bool IsFailure => !IsSuccess;
        public bool IsSuccess { get; private set; }
        public Exception? Exception { get; private set; }
        public T Value { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Result(bool isSucces, Exception? exception = null)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            IsSuccess = isSucces;
#pragma warning disable CS8601 // Possible null reference assignment.
            Value = default(T);
#pragma warning restore CS8601 // Possible null reference assignment.
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




