﻿using AutoMapper;
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
                InitializeLocations(cfg);
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
        }
    }
}
