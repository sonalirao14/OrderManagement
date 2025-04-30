using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Core.Interfaces;

namespace Infrastructure.Logging
{
    internal class CustomLogger : ILogger

    {
        //private readonly string _categoryName;

        //public CustomLogger(string categoryName)
        //{
        //    _categoryName = categoryName;
        //}

        //public IDisposable BeginScope<TState>(TState state) => null;

        //public bool IsEnabled(LogLevel logLevel) => true; // Log all levels

        //public void Log<TState>(
        //    LogLevel logLevel,
        //    EventId eventId,
        //    TState state,
        //    Exception exception,
        //    Func<TState, Exception, string> formatter)
        //{
        //    if (!IsEnabled(logLevel))
        //        return;

        //    var message = formatter(state, exception);
        //    var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {message}";

        //    if (exception != null)
        //    {
        //        logLine += Environment.NewLine + exception.ToString();
        //    }

        //    // Write to console instead of file
        //    Console.WriteLine(logLine);
        //}

        private readonly string _categoryName;
        private readonly CustomLoggerConfiguration _config;

        public CustomLogger(string categoryName, CustomLoggerConfiguration config)
        {
            _categoryName = categoryName;
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IDisposable BeginScope<TState>(TState state) => null; // Scopes not implemented for simplicity

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _config.MinimumLogLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var logLine = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {message}";

            if (exception != null)
            {
                logLine += Environment.NewLine + exception.ToString();
            }

            logLine += Environment.NewLine;

            // Log to console
            if (_config.LogToConsole)
            {
                Console.Write(logLine);
            }

            //// Log to file
            //if (_config.LogToFile)
            //{
            //    try
            //    {
            //        // Ensure directory exists
            //        var directory = Path.GetDirectoryName(_config.FilePath);
            //        if (!string.IsNullOrEmpty(directory))
            //        {
            //            Directory.CreateDirectory(directory);
            //        }
            //        File.AppendAllText(_config.FilePath, logLine);
            //    }
            //    catch (Exception ex)
            //    {
            //        // Fallback to console if file writing fails
            //        Console.WriteLine($"Failed to write to file: {ex.Message}");
            //    }
            //}

            //private readonly string _categoryName;

            //public CustomLogger(string categoryName)
            //{
            //    _categoryName = categoryName;
            //}

            //private void Log(string level, string message, Exception exception = null)
            //{
            //    var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {_categoryName}: {message}";

            //    if (exception != null)
            //    {
            //        logLine += Environment.NewLine + exception;
            //    }

            //    Console.WriteLine(logLine);
            //}

            //public void LogInformation(string message) => Log("Information", message);
            //public void LogWarning(string message) => Log("Warning", message);
            //public void LogError(string message, Exception exception = null) => Log("Error", message, exception);
            //public void LogDebug(string message) => Log("Debug", message);
        }
    }
}