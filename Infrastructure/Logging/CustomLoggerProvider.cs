using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging
{
    internal class CustomLoggerProvider : ILoggerProvider
    {
        //public CustomLoggerProvider()
        //{
        //    // No initialization needed
        //}
        //public ILogger CreateLogger(string categoryName)
        //{
        //    return new CustomLogger(categoryName);
        //}
        //public void Dispose()
        //{
        //    // No resources to dispose
        //}
        private readonly CustomLoggerConfiguration _config;

        public CustomLoggerProvider(CustomLoggerConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName, _config);
        }

        public void Dispose()
        {
            // No resources to dispose for now
        }
    }
}
