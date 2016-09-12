using System;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for location entities used with create and update controller commands.
    /// </summary>
    public class LocationCommandDto : LocationBaseDto
    {
        public Guid TenantId { get; set; }

        public string Name { get; set; }

        public Guid LocationTypeId { get; set; }

        public Guid? ParentId { get; set; }
    }
}
