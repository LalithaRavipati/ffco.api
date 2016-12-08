using System;
using AutoMapper;
using Hach.Fusion.Core.Business.Spatial;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Dtos.Dashboards;
using Hach.Fusion.FFCO.Core.Dtos.LimitTypes;
using Hach.Fusion.FFCO.Core.Entities;

namespace Hach.Fusion.FFCO.Business
{
    /// <summary>
    /// Manages mapping state information between classes.
    /// </summary>
    /// <remarks>
    /// This class maps properties in one class to properties in another using AutoMapper
    /// (see automapper.org). The mapping between properties is estalished by the Initialize
    /// method, which must be called before information is moved from one class to another.
    /// The initialization must be called only once.
    /// </remarks>
    public static class MappingManager
    {
        /// <summary>
        /// Gets or sets a flag indicating whether the <see cref="Initialize"/> method has been called.
        /// </summary>
        /// <value>Flag indicating whether the <see cref="Initialize"/> method has been called.</value>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// AutoMapper interface used for perfoming mapping.
        /// </summary>
        public static IMapper AutoMapper { get; private set; }

        /// <summary>
        /// Initialize object mapping.
        /// </summary>
        /// <remarks>
        /// This method should only be called once.
        /// </remarks>
        public static void Initialize()
        {
            // Can only initialize one time
            if (IsInitialized)
                return;

            IsInitialized = true;

            // Initialize groups of mapping classes
            var config = new MapperConfiguration(cfg =>
            {
                InitializeMessageTypes(cfg);
                InitializeInAppMessages(cfg);
                InitializeChemicalFormTypes(cfg);
                InitializeUnitTypeGroups(cfg);
                InitializeUnitTypes(cfg);
                InitializeParameterTypes(cfg);
                InitializeParameterValidRanges(cfg);
                InitializeLocations(cfg);
                InitializeLocationLogEntries(cfg);
                InitializeLocationTypes(cfg);
                InitializeParameters(cfg);
                InitializeDashboards(cfg);
                InitializeDashboardOptions(cfg);
                InitializeTenants(cfg);
                InitializeUsers(cfg);
                InitializeLimitTypes(cfg);

            });

            // Make sure the mapping is valid
            config.AssertConfigurationIsValid();

            AutoMapper = config.CreateMapper();
        }

        private static void InitializeMessageTypes(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<MessageType, MessageTypeQueryDto>();
        }

        private static void InitializeInAppMessages(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<InAppMessage, InAppMessageQueryDto>();

            cfg.CreateMap<InAppMessage, InAppMessageCommandDto>()
                .ForSourceMember(x => x.Body, opt => opt.Ignore())
                .ForSourceMember(x => x.DateRead, opt => opt.Ignore())
                .ForSourceMember(x => x.DateReceived, opt => opt.Ignore())
                .ForSourceMember(x => x.DateTimeSent, opt => opt.Ignore())
                .ForSourceMember(x => x.MessageType, opt => opt.Ignore())
                .ForSourceMember(x => x.MessageTypeId, opt => opt.Ignore())
                .ForSourceMember(x => x.SenderId, opt => opt.Ignore())
                .ForSourceMember(x => x.Subject, opt => opt.Ignore())
                .ForSourceMember(x => x.User, opt => opt.Ignore())
                .ForSourceMember(x => x.UserId, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore());

            cfg.CreateMap<InAppMessageCommandDto, InAppMessage>()
                .ForMember(x => x.Body, opt => opt.Ignore())
                .ForMember(x => x.DateRead, opt => opt.Ignore())
                .ForMember(x => x.DateReceived, opt => opt.Ignore())
                .ForMember(x => x.DateTimeSent, opt => opt.Ignore())
                .ForMember(x => x.MessageType, opt => opt.Ignore())
                .ForMember(x => x.MessageTypeId, opt => opt.Ignore())
                .ForMember(x => x.SenderId, opt => opt.Ignore())
                .ForMember(x => x.Subject, opt => opt.Ignore())
                .ForMember(x => x.User, opt => opt.Ignore())
                .ForMember(x => x.UserId, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Location entity and DTOs.
        /// </summary>
        private static void InitializeLocations(IProfileExpression cfg)
        {
            cfg.CreateMap<Location, LocationQueryDto>()
                .ForMember(x => x.Point, opt => opt.MapFrom(src => src.Geography.ConvertDbGeographyToPoint()));

            cfg.CreateMap<Location, LocationCommandDto>()
                .ForSourceMember(x => x.Parent, opt => opt.Ignore())
                .ForSourceMember(x => x.Locations, opt => opt.Ignore())
                .ForSourceMember(x => x.LocationType, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ProductOfferingTenantLocations, opt => opt.Ignore())
                .ForMember(x => x.Point, opt => opt.MapFrom(src => src.Geography.ConvertDbGeographyToPoint()));

            cfg.CreateMap<Location, LocationParentDto>()
                .ForSourceMember(x => x.Locations, opt => opt.Ignore())
                .ForMember(x => x.Point, opt => opt.MapFrom(src => src.Geography.ConvertDbGeographyToPoint()));

            cfg.CreateMap<LocationCommandDto, Location>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Parent, opt => opt.Ignore())
                .ForMember(x => x.Locations, opt => opt.Ignore())
                .ForMember(x => x.LocationType, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore())
                .ForMember(x => x.ProductOfferingTenantLocations, opt => opt.Ignore())
                .ForMember(x => x.Geography, opt => opt.MapFrom(src => src.Point.ConvertPointToDbGeography()));
        }

        /// <summary>
        /// Configures AutoMapper for converting between Location Log Entries and DTOs.
        /// </summary>
        /// <param name="cfg">Configuration for profile-specific maps.</param>
        private static void InitializeLocationLogEntries(IProfileExpression cfg)
        {
            cfg.CreateMap<LocationLogEntry, LocationLogEntryQueryDto>();

            cfg.CreateMap<LocationLogEntryCommandDto, LocationLogEntry>()
                .ForMember(x => x.Location, opt => opt.Ignore())
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore());

            cfg.CreateMap<LocationLogEntry, LocationLogEntryCommandDto>()
                .ForSourceMember(x => x.Location, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Location Type entity and DTOs.
        /// </summary>
        private static void InitializeLocationTypes(IProfileExpression cfg)
        {
            cfg.CreateMap<LocationType, LocationTypeQueryDto>();

            cfg.CreateMap<LocationTypeCommandDto, LocationType>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore());

            cfg.CreateMap<LocationType, LocationTypeCommandDto>()
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Parameter entity and DTO.
        /// </summary>
        private static void InitializeParameters(IProfileExpression cfg)
        {
            cfg.CreateMap<Parameter, ParameterDto>();
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Parameter Valid Range entity and DTO.
        /// </summary>
        private static void InitializeParameterValidRanges(IProfileExpression cfg)
        {
            cfg.CreateMap<ParameterValidRange, ParameterValidRangeQueryDto>();
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Unit Type Group entity and DTO.
        /// </summary>
        private static void InitializeUnitTypeGroups(IProfileExpression cfg)
        {
            cfg.CreateMap<UnitTypeGroup, UnitTypeGroupQueryDto>();
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Unit Type entity and DTOs.
        /// </summary>
        private static void InitializeUnitTypes(IProfileExpression cfg)
        {
            cfg.CreateMap<UnitType, UnitTypeQueryDto>();

            cfg.CreateMap<UnitType, UnitTypeCommandDto>()
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore());

            cfg.CreateMap<UnitTypeCommandDto, UnitType>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.UnitTypeGroup, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Parameter Type entity and DTO.
        /// </summary>
        private static void InitializeParameterTypes(IProfileExpression cfg)
        {
            cfg.CreateMap<ParameterType, ParameterTypeDto>();
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Dashboard entity and DTOs.
        /// </summary>
        private static void InitializeDashboards(IProfileExpression cfg)
        {
            cfg.CreateMap<Dashboard, DashboardQueryDto>();

            cfg.CreateMap<Dashboard, DashboardCommandDto>()
                .ForSourceMember(x => x.OwnerUserId, opt => opt.Ignore())
                .ForSourceMember(x => x.OwnerUser, opt => opt.Ignore())
                .ForSourceMember(x => x.Tenant, opt => opt.Ignore())
                .ForSourceMember(x => x.DashboardOption, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore());

            cfg.CreateMap<DashboardCommandDto, Dashboard>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.OwnerUserId, opt => opt.Ignore())
                .ForMember(x => x.OwnerUser, opt => opt.Ignore())
                .ForMember(x => x.Tenant, opt => opt.Ignore())
                .ForMember(x => x.DashboardOption, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Dashboard Option entity and DTOs.
        /// </summary>
        private static void InitializeDashboardOptions(IProfileExpression cfg)
        {
            cfg.CreateMap<DashboardOption, DashboardOptionQueryDto>();

            cfg.CreateMap<DashboardOption, DashboardOptionCommandDto>()
                .ForSourceMember(x => x.Tenant, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore());

            cfg.CreateMap<DashboardOptionCommandDto, DashboardOption>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Tenant, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Tenant entity and DTO.
        /// </summary>
        private static void InitializeTenants(IProfileExpression cfg)
        {
            cfg.CreateMap<Tenant, TenantDto>();
        }

        /// <summary>
        /// Configure AutoMapper for converting between the User entity and DTO.
        /// </summary>
        private static void InitializeUsers(IProfileExpression cfg)
        {
            cfg.CreateMap<User, UserDto>()
                .ForSourceMember(x => x.Tenants, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Limit Type entity and DTOs.
        /// </summary>
        private static void InitializeLimitTypes(IProfileExpression cfg)
        {
            cfg.CreateMap<LimitType, LimitTypeQueryDto>();

            cfg.CreateMap<LimitType, LimitTypeCommandDto>()
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore());

            cfg.CreateMap<LimitTypeCommandDto, LimitType>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore());
        }
        /// <summary>
        /// Configure AutoMapper for converting between the Chemical Form Type entity and DTOs.
        /// </summary>
        private static void InitializeChemicalFormTypes(IProfileExpression cfg)
        {
            cfg.CreateMap<ChemicalFormType, ChemicalFormTypeQueryDto>();
        }
    }
}
