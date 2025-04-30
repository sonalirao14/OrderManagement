using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Logging
{
    public class CustomLoggerConfiguration
    {
        public bool LogToConsole { get; set; } = true;
        public bool LogToFile { get; set; } = false;
        //public string FilePath { get; set; } = "logs/app.log";
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    }
}
