using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Logging
{
    public static class CustomLoggerExtension
    {
        //public static ILoggingBuilder AddCustomLogger(this ILoggingBuilder builder)
        //{
        //    builder.Services.AddSingleton<ILoggerProvider, CustomLoggerProvider>();
        //    return builder;
        //}
        public static ILoggingBuilder AddCustomLogger(this ILoggingBuilder builder)
        {
            return AddCustomLogger(builder, new CustomLoggerConfiguration());
        }

        public static ILoggingBuilder AddCustomLogger(this ILoggingBuilder builder, CustomLoggerConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            builder.Services.AddSingleton<ILoggerProvider, CustomLoggerProvider>(sp => new CustomLoggerProvider(config));
            return builder;
        }

        public static ILoggingBuilder AddCustomLogger(this ILoggingBuilder builder, Action<CustomLoggerConfiguration> configure)
        {
            var config = new CustomLoggerConfiguration();
            configure?.Invoke(config);
            return AddCustomLogger(builder, config);
        }
    }
}
