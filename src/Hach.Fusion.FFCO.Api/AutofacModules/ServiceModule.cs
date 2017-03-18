using Autofac;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Database;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Azure.Blob;
using Hach.Fusion.Data.Azure.DocumentDB;
using Hach.Fusion.Data.Azure.Queue;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Database.Interfaces;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.FFCO.Api.Notifications;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Notifications;
using Hach.Fusion.FFCO.Business.Validators;
using System;
using System.Configuration;
using System.Security.Claims;

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
            builder.Register(c => new DocumentDbRepository<UploadTransaction>(endpoint, authKey, databaseId, collectionId))
                .As<IDocumentDbRepository<UploadTransaction>>().InstancePerLifetimeScope();

            // OData Helper
            builder.RegisterType<ODataHelper>().As<IODataHelper>().InstancePerLifetimeScope();

            // Claims Transformation
            builder.Register(c => new FusionContextFactory(connectionString)).AsSelf();
            builder.RegisterType<ClaimsTransformer>().AsSelf().As<ClaimsAuthenticationManager>().InstancePerLifetimeScope();

            builder.RegisterType<BlobManager>().AsSelf().As<IBlobManager>();
            builder.RegisterType<QueueManager>().AsSelf().As<IQueueManager>();

            builder.RegisterType<NotificationSender>().AsSelf().As<INotificationSender>();

            // LocationParameters
            builder.RegisterType<LocationFacade>().As<IFacadeWithCruModels<LocationBaseDto, LocationBaseDto,
                LocationQueryDto, Guid>>();

            builder.RegisterType<LocationTypeFacade>().As<IFacadeWithCruModels<LocationTypeBaseDto, LocationTypeBaseDto,
                LocationTypeQueryDto, Guid>>();

            builder.RegisterType<LocationLogEntryFacade>().As<IFacadeWithCruModels<LocationLogEntryBaseDto, LocationLogEntryBaseDto,
                LocationLogEntryQueryDto, Guid>>();

            builder.RegisterType<UnitTypeFacade>().As<IFacadeWithCruModels<UnitTypeBaseDto, UnitTypeBaseDto,
               UnitTypeQueryDto, Guid>>();
            builder.RegisterType<UnitTypeGroupFacade>().As<IFacadeWithCruModels<UnitTypeGroupBaseDto, UnitTypeGroupBaseDto,
               UnitTypeGroupQueryDto, Guid>>();

            builder.RegisterType<LocationValidator>().As<IFFValidator<LocationBaseDto>>();
            builder.RegisterType<LocationLogEntryValidator>().As<IFFValidator<LocationLogEntryBaseDto>>();
            builder.RegisterType<LocationTypeValidator>().As<IFFValidator<LocationTypeBaseDto>>();
            builder.RegisterType<UnitTypeValidator>().As<IFFValidator<UnitTypeBaseDto>>();
            builder.RegisterType<UnitTypeGroupValidator>().As<IFFValidator<UnitTypeGroupBaseDto>>();

            builder.RegisterType<ParameterTypeFacade>().As<IFacade<ParameterTypeQueryDto, Guid>>();
            builder.RegisterType<ParameterFacade>().As<IFacade<ParameterQueryDto, Guid>>();

            builder.RegisterType<ParameterValidRangeFacade>().As<IFacade<ParameterValidRangeQueryDto, Guid>>();

            builder.RegisterType<DashboardFacade>().As<IFacadeWithCruModels<DashboardBaseDto, DashboardBaseDto,
                DashboardQueryDto, Guid>>();
            builder.RegisterType<DashboardValidator>().As<IFFValidator<DashboardBaseDto>>();

            builder.RegisterType<DashboardOptionFacade>().As<IFacadeWithCruModels<DashboardOptionBaseDto, DashboardOptionBaseDto,
                DashboardOptionQueryDto, Guid>>();
            builder.RegisterType<DashboardOptionValidator>().As<IFFValidator<DashboardOptionBaseDto>>();

            builder.RegisterType<LimitTypeValidator>().As<IFFValidator<LimitTypeBaseDto>>();
            builder.RegisterType<LimitTypeFacade>().As<IFacadeWithCruModels<LimitTypeBaseDto, LimitTypeBaseDto,
               LimitTypeQueryDto, Guid>>();

            //ChemicalFormsTypes
            builder.RegisterType<ChemicalFormTypesFacade>().As<IFacadeWithCruModels<ChemicalFormTypeBaseDto, ChemicalFormTypeBaseDto,
               ChemicalFormTypeQueryDto, Guid>>();

            builder.RegisterType<InAppMessageFacade>().As<IInAppMessageFacade>();
            builder.RegisterType<InAppMessageValidator>().As<IFFValidator<InAppMessageBaseDto>>();

            builder.RegisterType<OperationConfigurationsFacade>().As<IOperationConfigurationsFacade>();

            builder.RegisterType<NotificationsFacade>().As<INotificationsFacade>();
            builder.RegisterType<NotificationValidator>().As<IFFValidator<GenericNotificationDto>>();

            builder.RegisterType<FileFacade>().As<IFileFacade>();

            base.Load(builder);
        }
    }
}