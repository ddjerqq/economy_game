#pragma warning disable CS1591
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using Domain.Common;
using FluentValidation.AspNetCore;
using Infrastructure.JsonConverters;
using Microsoft.AspNetCore.ResponseCompression;
using Persistence;
using WebApi.Filters;
using WebApi.HealthChecks;
using WebApi.Middleware;
using ZymLabs.NSwag.FluentValidation;

namespace WebApi.Config;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ConfigureWebApi : ConfigurationBase
{
    private static readonly string[] CompressionTypes = ["application/octet-stream"];

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAntiforgery();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("db")
            .AddCheck<TestHealthCheck>("Test");

        services.AddScoped<GlobalExceptionHandlerMiddleware>();
        services.AddHttpContextAccessor();

        services.Configure<RouteOptions>(x =>
        {
            x.LowercaseUrls = true;
            x.LowercaseQueryStrings = true;
            x.AppendTrailingSlash = false;
        });

        services.AddIdempotency();
        services.AddSpaApiFallbackMiddleware();

        services.AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters();

        services.AddScoped<FluentValidationSchemaProcessor>(sp =>
        {
            var validationRules = sp.GetService<IEnumerable<FluentValidationRule>>();
            var loggerFactory = sp.GetService<ILoggerFactory>();
            return new FluentValidationSchemaProcessor(sp, validationRules, loggerFactory);
        });

        services
            .AddControllers(o =>
            {
                o.Filters.Add<ResponseTimeFilter>();
                o.Filters.Add<SetClientIpAddressFilter>();
                o.Filters.Add<FluentValidationFilter>();
                o.RespectBrowserAcceptHeader = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new UlidToStringJsonConverter());
            });

        services.AddCors(options =>
        {
            var webAppDomain = "WEB_APP__DOMAIN".FromEnvRequired();

            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader();
                policy.WithOrigins(webAppDomain);
                policy.WithMethods("GET", "POST", "HEAD");
                policy.AllowCredentials();
            });
        });

        services.AddResponseCaching();
        services.AddResponseCompression(o =>
        {
            o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(CompressionTypes);
            o.Providers.Add<GzipCompressionProvider>();
            o.Providers.Add<BrotliCompressionProvider>();
        });
    }
}