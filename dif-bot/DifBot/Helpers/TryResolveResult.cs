﻿using System;

namespace DifBot.Helpers
{
    public class TryResolveResult<T>
    {
        private T _value = default!;

        public bool Resolved { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public T Value
        {
            get => _value ?? throw new NullReferenceException();
            private set => _value = value;
        }

        private TryResolveResult(bool resolved, string errorMessage)
        {
            Resolved = resolved;
            ErrorMessage = errorMessage;
        }

        private TryResolveResult(T value)
        {
            Resolved = true;
            Value = value;
        }

        public static TryResolveResult<T> FromValue(T result)
        {
            return new TryResolveResult<T>(result);
        }

        public static TryResolveResult<T> FromError(string errorMessage)
        {
            return new TryResolveResult<T>(false, errorMessage);
        }
    }
}
