using System;
using System.Collections.Generic;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for location entities used with query controller commands.
    /// </summary>
    public class LocationQueryDto : LocationBaseDto
    {
        public string Name { get; set; }

        public Guid LocationTypeId { get; set; }
        public Guid? ParentId { get; set; }
            
        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }

        // Expandable properties
        public virtual LocationTypeQueryDto LocationType { get; set; }

        // Having an expandable Parent property with an entity will result in the following error, so don't uncomment this property
        // The complex type 'Hach.Fusion.FFCO.Entities.Location' has a reference to itself through the property 'Parent'. 
        // A recursive loop of complex types is not allowed. Parameter name: propertyInfo
        // public virtual Location Parent { get; set; }

        // Having an expandable Parent property with a DTO will result in a stack overflow error, so don't uncomment this property
        public virtual LocationParentDto Parent { get; set; }

        // Expandable One-to-Many relationships
        public ICollection<LocationQueryDto> Locations { get; set; }
    }
}
