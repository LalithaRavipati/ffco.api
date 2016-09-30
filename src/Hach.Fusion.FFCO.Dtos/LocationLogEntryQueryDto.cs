using System;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for Location Log Entry entities used with query controller commands.
    /// </summary>
    public class LocationLogEntryQueryDto : LocationLogEntryBaseDto
    {
        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
