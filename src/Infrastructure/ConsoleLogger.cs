﻿using System;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public static class ConsoleLogger
    {
        public static ILogger<T> Create<T>()
        {
            var logger = new ConsoleLogger<T>();
            return logger;
        }
    }

    public class ConsoleLogger<T> : ILogger<T>, IDisposable
    {
        private readonly Action<string> output = Console.WriteLine;

        public void Dispose()
        {
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter) => output(formatter(state, exception));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => this;
    }
}