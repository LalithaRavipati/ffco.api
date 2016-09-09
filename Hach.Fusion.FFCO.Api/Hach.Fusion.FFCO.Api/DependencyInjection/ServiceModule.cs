using System;
using System.Configuration;
using Autofac;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
using Hach.Fusion.FFCO.Dtos;

namespace Hach.Fusion.FFCO.Api.DependencyInjection
{
    /// <summary>
    /// Service module that provides for dependency injection.
    /// </summary>
    public class ServiceModule : Module
    {
        /// <summary>
        /// Registers types for dependency injection.
        /// </summary>
        /// <param name="builder">Container used to hold dependency injection registration information.</param>
        protected override void Load(ContainerBuilder builder)
        {
            // Contexts
            builder.Register(
                c => new DataContext(ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString))
                .AsSelf()
                .As<DataContext>()
                .InstancePerLifetimeScope();

            // OData Helper
            builder.RegisterType<ODataHelper>().As<IODataHelper>().InstancePerLifetimeScope();

            // Claims Transformation
            builder.RegisterType<ClaimsTransformationMiddleware>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<ClaimsTransformer>().AsSelf().InstancePerLifetimeScope();

            // Locations
            builder.RegisterType<LocationFacade>().As<IFacadeWithCruModels<LocationCommandDto, LocationCommandDto,
                LocationQueryDto, Guid>>();
            builder.RegisterType<LocationValidator>().As<IFFValidator<LocationCommandDto>>();

            base.Load(builder);
        }
    }
}