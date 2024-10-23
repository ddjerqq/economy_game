using Application;
using dotenv.net;
using FluentValidation;
using Infrastructure.Config;
using SerilogTracing;
using WebApi;

// set global fluent validation cascade mode to stop
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

// load .env
var solutionDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent;
DotEnv.Fluent()
    .WithTrimValues()
    .WithEnvFiles($"{solutionDir}/.env")
    .WithOverwriteExistingVars()
    .Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

// for serilog - seq tracing
using var _ = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests(options => options.IncomingTraceParent = IncomingTraceParent.Trust)
    .Instrument.SqlClientCommands()
    .TraceToSharedLogger();

builder.WebHost.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
builder.WebHost.UseStaticWebAssets();

// service registration from configurations.
ConfigurationBase.ConfigureServicesFromAssemblies(builder.Services, [
    nameof(Domain), nameof(Application), nameof(Infrastructure),
    nameof(Persistence), nameof(WebApi),
]);

var app = builder.Build();

app.UseConfiguredSerilogRequestLogging();
await app.MigrateDatabaseAsync();
app.UseApplicationMiddleware();

app.Run();