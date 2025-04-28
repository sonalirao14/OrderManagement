using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public class LoggingService: ILoggingService
    {
       
         private readonly ILogger _logger;

        public LoggingService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LoggingService>();
        }

        public async Task LogDebugAsync(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
            await Task.CompletedTask;
        }

        public async Task LogInformationAsync(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
            await Task.CompletedTask;
        }

        public async Task LogWarningAsync(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
            await Task.CompletedTask;
        }

        public async Task LogErrorAsync(Exception? exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
            await Task.CompletedTask;
        }

        public async Task LogFatalAsync(Exception? exception, string message, params object[] args)
        {
            _logger.LogCritical(exception, message, args);
            await Task.CompletedTask;
        
         }
    }
    
    }

