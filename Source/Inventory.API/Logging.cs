using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using System.Reflection;

namespace Inventory.API;

public class LoggingConfig
{
    public const string LoggerTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] ({SourceContext:l}) ({ThreadId}) {Indent:l}{Message}{NewLine}{Exception}";
    public bool WriteToConsole { get; set; }
    public bool WriteToFile { get; set; }
    public string FileLocation { get; set; } = default!;
}

public static class LoggingInfrastructure
{
    public static IHostBuilder ConfigureLoggingInfrastructure(this IHostBuilder builder, string? assemblyName = null)
    {
        if (string.IsNullOrEmpty(assemblyName))
        {
            assemblyName = Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-");
        }

        builder.UseSerilog((context, configuration) =>
        {
            LoggingConfig? loggingConfig = context.Configuration.GetSection("Logging").Get<LoggingConfig>();

            configuration
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("ServiceName", assemblyName ?? "inventory")
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }));

            if (loggingConfig != null && loggingConfig.WriteToConsole)
            {
                configuration.WriteTo.Console(Serilog.Events.LogEventLevel.Verbose, LoggingConfig.LoggerTemplate);
            }

            if (loggingConfig != null && loggingConfig.WriteToFile)
            {
                configuration.WriteTo.Async(a => a.File($"{loggingConfig.FileLocation}/logs.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, flushToDiskInterval: TimeSpan.FromSeconds(1), outputTemplate: LoggingConfig.LoggerTemplate));
            }

            configuration
                .ReadFrom.Configuration(context.Configuration);

        });

        return builder;
    }
}