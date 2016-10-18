using System;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for location entities used with query controller commands.
    /// </summary>
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
