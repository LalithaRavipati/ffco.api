using AutoMapper;
using Hach.Fusion.Core.Business.Spatial;
using Hach.Fusion.FFCO.Dtos;
using Hach.Fusion.FFCO.Entities;

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
                InitializeUnitTypes(cfg);
                InitializeParameterTypes(cfg);
                InitializeLocations(cfg);
                InitializeParameters(cfg);
            });

            // Make sure the mapping is valid
            config.AssertConfigurationIsValid();

            AutoMapper = config.CreateMapper();
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Location entity and Dtos.
        /// </summary>
        private static void InitializeLocations(IProfileExpression cfg)
        {
            cfg.CreateMap<Location, LocationQueryDto>();
                //.ForMember(x => x.Name, opt => opt.Ignore());

            
            cfg.CreateMap<LocationType, LocationTypeQueryDto>();

            cfg.CreateMap<UnitType, UnitTypeQueryDto>();
            cfg.CreateMap<UnitTypeGroup, UnitTypeGroupQueryDto>();

            cfg.CreateMap<Location, LocationCommandDto>()
                .ForSourceMember(x => x.Parent, opt => opt.Ignore())
                .ForSourceMember(x => x.Locations, opt => opt.Ignore())
                .ForSourceMember(x => x.LocationType, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedById, opt => opt.Ignore())
                .ForSourceMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForSourceMember(x => x.ModifiedOn, opt => opt.Ignore())
                .ForSourceMember(x => x.IsDeleted, opt => opt.Ignore())
                .ForSourceMember(x => x.Geography, opt => opt.Ignore())
                .ForSourceMember(x => x.ProductOfferingTenantLocations, opt => opt.Ignore());            
               

            cfg.CreateMap<LocationCommandDto, Location>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Parent, opt => opt.Ignore())
                .ForMember(x => x.Locations, opt => opt.Ignore())
                .ForMember(x => x.LocationType, opt => opt.Ignore())
                .ForMember(x => x.CreatedById, opt => opt.Ignore())
                .ForMember(x => x.CreatedOn, opt => opt.Ignore())
                .ForMember(x => x.ModifiedById, opt => opt.Ignore())
                .ForMember(x => x.ModifiedOn, opt => opt.Ignore())
                .ForMember(x => x.IsDeleted, opt => opt.Ignore())
                .ForMember(x => x.Geography, opt => opt.Ignore())
                .ForMember(x => x.ProductOfferingTenantLocations, opt => opt.Ignore());

        }

        /// <summary>
        /// Initializes mapping location entities to Data Transfer Objects (DTOs) for commands and vice versa.
        /// </summary>
        private static void InitializeLocationForCommands()
        private static void InitializeLocations(IProfileExpression cfg)
        {
            cfg.CreateMap<Location, LocationQueryDto>()
                .ForMember(x => x.InternalName, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the Parameter entity and Dtos.
        /// </summary>
        private static void InitializeParameters(IProfileExpression cfg)
        {
            cfg.CreateMap<Parameter, ParameterDto>()
                .ForSourceMember(x => x.IsDeleted, opt => opt.Ignore());
        }

        /// <summary>
        /// Configure AutoMapper for converting between the UnitType entity and Dtos.
        /// </summary>
        private static void InitializeUnitTypes(IProfileExpression cfg)
        {
            cfg.CreateMap<UnitType, UnitTypeDto>();
        }

        /// <summary>
        /// Configure AutoMapper for converting between the ParameterType entity and Dtos.
        /// </summary>
        private static void InitializeParameterTypes(IProfileExpression cfg)
        {
            cfg.CreateMap<ParameterType, ParameterTypeDto>();
        }
    }
}
