using System;
using System.Configuration;
using Autofac;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Azure.Blob;
using Hach.Fusion.Core.Azure.Queue;
using Hach.Fusion.Core.Azure.DocumentDB;
using Hach.Fusion.Core.Business.Database;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Api.Notifications;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Validators;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Dtos.Dashboards;
using Hach.Fusion.FFCO.Core.Dtos.LimitTypes;
using Hach.Fusion.FFCO.Core.Dtos.LocationType;

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
            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;

            var databaseId = ConfigurationManager.AppSettings["DocumentDBDatabase"];
            var collectionId = ConfigurationManager.AppSettings["DocumentDBCollection"];
            var endpoint = ConfigurationManager.AppSettings["DocumentDBEndpoint"];
            var authKey = ConfigurationManager.AppSettings["DocumentDBAuthKey"];

            // Contexts
            builder.Register(c => new DataContext(connectionString)).AsSelf().As<DataContext>().InstancePerLifetimeScope();
            builder.Register(c => new DocumentDBRepository<UploadTransaction>(endpoint, authKey, databaseId, collectionId))
                .As<IDocumentDBRepository<UploadTransaction>>().InstancePerLifetimeScope();

            // OData Helper
            builder.RegisterType<ODataHelper>().As<IODataHelper>().InstancePerLifetimeScope();

            // Claims Transformation
            builder.Register(c => new FusionContextFactory(connectionString)).AsSelf();
            builder.RegisterType<RoleClaimsTransformer>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<RoleClaimsTransformationMiddleware>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<BlobManager>().AsSelf().As<IBlobManager>();
            builder.RegisterType<QueueManager>().AsSelf().As<IQueueManager>();

            builder.RegisterType<NotificationSender>().AsSelf().As<INotificationSender>();

            // LocationParameters
            builder.RegisterType<LocationFacade>().As<IFacadeWithCruModels<LocationCommandDto, LocationCommandDto,
                LocationQueryDto, Guid>>();

            builder.RegisterType<LocationTypeFacade>().As<IFacadeWithCruModels<LocationTypeCommandDto, LocationTypeCommandDto,
                LocationTypeQueryDto, Guid>>();

            builder.RegisterType<LocationLogEntryFacade>().As<IFacadeWithCruModels<LocationLogEntryCommandDto, LocationLogEntryCommandDto,
                LocationLogEntryQueryDto, Guid>>();

            builder.RegisterType<UnitTypeFacade>().As<IFacadeWithCruModels<UnitTypeCommandDto, UnitTypeCommandDto,
               UnitTypeQueryDto, Guid>>();
            builder.RegisterType<UnitTypeGroupFacade>().As<IFacadeWithCruModels<UnitTypeGroupQueryDto, UnitTypeGroupQueryDto,
               UnitTypeGroupQueryDto, Guid>>();

            builder.RegisterType<LocationValidator>().As<IFFValidator<LocationCommandDto>>();
            builder.RegisterType<LocationLogEntryValidator>().As<IFFValidator<LocationLogEntryCommandDto>>();
            builder.RegisterType<LocationTypeValidator>().As<IFFValidator<LocationTypeCommandDto>>();
            builder.RegisterType<UnitTypeValidator>().As<IFFValidator<UnitTypeCommandDto>>();
            builder.RegisterType<UnitTypeGroupValidator>().As<IFFValidator<UnitTypeGroupQueryDto>>();

            builder.RegisterType<ParameterTypeFacade>().As<IFacade<ParameterTypeDto, Guid>>();
            builder.RegisterType<ParameterFacade>().As<IFacade<ParameterDto, Guid>>();

            builder.RegisterType<ParameterValidRangeFacade>().As<IFacade<ParameterValidRangeQueryDto, Guid>>();

            builder.RegisterType<DashboardFacade>().As<IFacadeWithCruModels<DashboardCommandDto, DashboardCommandDto,
                DashboardQueryDto, Guid>>();
            builder.RegisterType<DashboardValidator>().As<IFFValidator<DashboardCommandDto>>();

            builder.RegisterType<DashboardOptionFacade>().As<IFacadeWithCruModels<DashboardOptionCommandDto, DashboardOptionCommandDto,
                DashboardOptionQueryDto, Guid>>();
            builder.RegisterType<DashboardOptionValidator>().As<IFFValidator<DashboardOptionCommandDto>>();

            builder.RegisterType<LimitTypeValidator>().As<IFFValidator<LimitTypeCommandDto>>();
            builder.RegisterType<LimitTypeFacade>().As<IFacadeWithCruModels<LimitTypeCommandDto, LimitTypeCommandDto,
               LimitTypeQueryDto, Guid>>();

            //ChemicalFormsTypes
            builder.RegisterType<ChemicalFormTypesFacade>().As<IFacadeWithCruModels<ChemicalFormTypeQueryDto, ChemicalFormTypeQueryDto,
               ChemicalFormTypeQueryDto, Guid>>();

            builder.RegisterType<InAppMessageFacade>().As<IInAppMessageFacade>();
            builder.RegisterType<InAppMessageValidator>().As<IFFValidator<InAppMessageCommandDto>>();

            builder.RegisterType<PlantConfigurationsFacade>().As<IPlantConfigurationsFacade>();

            builder.RegisterType<NotificationsFacade>().As<INotificationsFacade>();
            builder.RegisterType<NotificationValidator>().As<IFFValidator<NotificationDto>>();

            base.Load(builder);
        }
    }
}