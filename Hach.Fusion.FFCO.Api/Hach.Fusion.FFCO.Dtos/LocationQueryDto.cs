using System;
using System.Collections.Generic;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for location entities used with query controller commands.
    /// </summary>
    public class LocationQueryDto : LocationBaseDto
    {
        public Guid TenantId { get; set; }

        public string InternalName { get; set; }

        public Guid LocationTypeId { get; set; }

        public Guid? ParentId { get; set; }

        public ICollection<LocationQueryDto> Locations { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
