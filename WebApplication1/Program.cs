using Infrastructure.Data;
using Infrastructure.DataBase;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Core.DTOs;
using WebApplication1.Controllers;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using Application.Validators;
using WebApplication1.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using WebApplication1.Services;
using Infrastructure.Caching;
using System.Text.Json;
using Infrastructure;
using Microsoft.Extensions.Logging.Console;




namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "CustomJwt";
                options.DefaultChallengeScheme = "CustomJwt";
            }).AddScheme<AuthenticationSchemeOptions, PassThroughAuthenticationHandler>("CustomJwt", null);

            //builder.Services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AdminOnly", policy =>
            //        policy.RequireRole("admin")
            //    );
            //});

            //builder.Services.AddLogging(logging =>
            //{
            //    logging.AddConsole();
            //    logging.SetMinimumLevel(LogLevel.Information);
            //});

            //change
            //builder.Logging.ClearProviders();
            //builder.Logging.AddJsonConsole(options =>
            //{
            //    options.JsonWriterOptions = new JsonWriterOptions { Indented = true };
            //});
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole(options => options.FormatterName = ConsoleFormatterNames.Simple);
                //logging.AddFilter("Microsoft", LogLevel.Warning);
                //logging.AddFilter("System", LogLevel.Warning);
                //logging.SetMinimumLevel(Enum.Parse<LogLevel>(
                //    builder.Configuration["Logging:LogLevel:Default"] ?? "Information"));
            });

            // Register dependencies
            builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);


            //builder.Services.AddSingleton<IDistributedCache, CustomRedisCache>();
            //builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            //{
            //    var logger = sp.GetRequiredService<ILogger<Program>>();
            //    try
            //    {
            //        //var config = ConfigurationOptions.Parse("Redis:Host");
            //        //config.DefaultDatabase = 1; // db=1 for .NET
            //        //config.ConnectTimeout = 5000;
            //        //config.AbortOnConnectFail = false;
            //        //return ConnectionMultiplexer.Connect(config);
            //        var redisHost = builder.Configuration["Redis:Host"] ?? "localhost:6379";
            //        var config = ConfigurationOptions.Parse(redisHost);
            //        config.DefaultDatabase = 1; // db=1 for .NET
            //        config.ConnectTimeout = 5000;
            //        config.AbortOnConnectFail = false;

            //        logger.LogInformation("Connecting to Redis at: {RedisHost}", redisHost);

            //        return ConnectionMultiplexer.Connect(config);
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.LogError(ex, "Failed to connect to Redis at startup. Caching will fallback to database.");
            //        throw;
            //    }
            //});

            //builder.Services.AddScoped<ProductRedisCache>();

            // Register Redis configuration and connection factory
            builder.Services.Configure<RedisConfiguration>(builder.Configuration.GetSection("Redis"));
            builder.Services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IRedisConnectionFactory>().GetConnection());

            // Register cache service
            builder.Services.AddScoped(typeof(ICacheService<>), typeof(RedisCacheService<>));
            //change
            builder.Services.AddSingleton<ILoggingService, LoggingService>();

            builder.Services.AddScoped<JwtSecurityTokenHandlerWrapper>();



            builder.Services.AddControllers();

            builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();
            builder.Services.AddFluentValidationAutoValidation();

            DotNetEnv.Env.Load();
            var connectionString = builder.Configuration["ConnectionStrings:default"];
            builder.Services.AddDbContext<AppDBContext>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 34))));

            //builder.Services.AddDbContext<AppDBContext>(options =>
            //      options.UseMySql(builder.Configuration.GetConnectionString("default"),
            //                       new MySqlServerVersion(new Version(8, 0, 34))));
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            builder.Services.AddScoped<IDBManager, DBManager>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            //builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
    });
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();
            app.UseCors("AllowAll");
         //change
            //app.UseMiddleware<ExceptionHandling>();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                dbContext.Database.Migrate();
            }


            app.UseHttpsRedirection();
            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthentication();
            //app.UseMiddleware<JwtMiddleware>();
            app.UseMiddleware<AuthContext>();
            app.UseAuthorization();
            app.UseMiddleware<ExceptionHandling>();
            app.MapControllers();

            app.Run();
        }

     
    }
}
