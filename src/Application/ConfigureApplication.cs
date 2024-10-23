using System.ComponentModel;
using Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ConfigureApplication : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAutoMapper(Constants.Assembly);
        services.AddValidatorsFromAssembly(Constants.Assembly, includeInternalTypes: true);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Constants.Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour<,>));
        });
    }
}