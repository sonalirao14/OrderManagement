using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public  interface ILoggingService
    {
        //void LogDebug(string message, params object[] args);
        //void LogInformation(string message, params object[] args);
        //void LogWarning(string message, params object[] args);
        //void LogError(Exception? exception, string message, params object[] args);
        //void LogFatal(Exception? exception, string message, params object[] args);
        Task LogDebugAsync(string message, params object[] args);
        Task LogInformationAsync(string message, params object[] args);
        Task LogWarningAsync(string message, params object[] args);
        Task LogErrorAsync(Exception? exception, string message, params object[] args);
        Task LogFatalAsync(Exception? exception, string message, params object[] args);
    }
}
