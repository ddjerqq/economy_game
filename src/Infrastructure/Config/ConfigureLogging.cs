using System.ComponentModel;
using Application;
using Application.Common;
using Destructurama;
using Domain.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Templates.Themes;
using SerilogTracing.Expressions;

namespace Infrastructure.Config;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ConfigureLogging : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .Configure()
            .CreateLogger();

        services.AddSerilog();
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
    }
}

public static class LoggingExt
{
    private const string OutputFormat =
        "[{Timestamp:dd-MM-yyyy HH:mm:ss.fff} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration Configure(this LoggerConfiguration config)
    {
        var logPath = "LOG__PATH".FromEnvRequired();

        return config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Destructure.UsingAttributes()
            .Enrich.WithProperty("Application", "tbc")
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithAssemblyName()
            .WriteTo.Debug()
            .WriteTo.Console(Formatters.CreateConsoleTextFormatter(TemplateTheme.Code))
            .WriteTo.File(logPath,
                outputTemplate: OutputFormat,
                flushToDiskInterval: TimeSpan.FromSeconds(10),
                fileSizeLimitBytes: 10_000_000,
                rollOnFileSizeLimit: true,
                rollingInterval: RollingInterval.Day);
    }

    public static void UseConfiguredSerilogRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.Logger = Log.Logger;
            options.IncludeQueryInRequestPath = true;
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("UserId", httpContext.User.GetId()?.ToString() ?? "unauthenticated");
                diagnosticContext.Set("ClientAddress", httpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diagnosticContext.Set("ClientUserAgent", (string?)httpContext.Request.Headers.UserAgent);
            };
        });
    }

    public static void UseConfiguredSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((_, configuration) => configuration.Configure());
    }
}