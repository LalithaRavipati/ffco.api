using System;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for Location entity parents used with query controller commands.
    /// </summary>
    /// <remarks>
    /// This DTO is the same as the <see cref="LocationQueryDto"/> except that it does not include
    /// the Locations property. It is used for referencing a Location's parent. If the Locations
    /// property were included, it would result in a circular reference and a stack overflow when
    /// retrieving Locations.
    /// </remarks>
    public class LocationParentDto : LocationBaseDto
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

        public virtual LocationParentDto Parent { get; set; }
    }
}
