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

namespace Hach.Fusion.FFCO.Api.AutofacModules
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
            /*builder.Register(
                c => new FFAAContext(ConfigurationManager.ConnectionStrings["IdSvrConnectionString"].ConnectionString))
                .AsSelf()
                .As<IIdentityContext>()
                .InstancePerLifetimeScope();*/

            // OData Helper
            builder.RegisterType<ODataHelper>().As<IODataHelper>().InstancePerLifetimeScope();

            // Claims Transformation
            builder.RegisterType<ClaimsTransformationMiddleware>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<ClaimsTransformer>().AsSelf().InstancePerLifetimeScope();

            // LocationParameters
            builder.RegisterType<LocationFacade>().As<IFacadeWithCruModels<LocationCommandDto, LocationCommandDto,
                LocationQueryDto, Guid>>();

            builder.RegisterType<LocationTypeFacade>().As<IFacadeWithCruModels<LocationTypeQueryDto, LocationTypeQueryDto,
                LocationTypeQueryDto, Guid>>();

            builder.RegisterType<LocationValidator>().As<IFFValidator<LocationCommandDto>>();
            builder.RegisterType<LocationTypeValidator>().As<IFFValidator<LocationTypeQueryDto>>();

            /*builder.RegisterType<UnitConverter>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<ChemicalFormConverter>().AsSelf().InstancePerLifetimeScope();*/

            base.Load(builder);
        }
    }
}