using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Base class for all Location Data Transfer Objects (DTOs).
    /// </summary>
    /// <remarks>
    /// This class doesn't provide state or actions. It is needed in order for OData
    /// to function properly. OData controllers must be associated with one and only one entity
    /// type. However, different entity types are allowed if they derive from the one entity.
    /// This DTO is intended to be the one entity. Other DTOs managed by the Location controller
    /// should derive from this DTO.
    /// </remarks>
    public class LocationBaseDto : FFDto<Guid>
    {
    }
}