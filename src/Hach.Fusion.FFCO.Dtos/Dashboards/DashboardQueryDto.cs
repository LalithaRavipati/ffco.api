using System;

namespace Hach.Fusion.FFCO.Dtos.Dashboards
{
    /// <summary>
    /// Data Transfer Object (DTO) for dashboard entities used with query controller commands.
    /// </summary>
    public class DashboardQueryDto : DashboardBaseDto
    {
        public Guid OwnerUserId { get; set; }
        public UserDto OwnerUser { get; set; }

        public Guid TenantId { get; set; }
        public TenantDto Tenant { get; set; }

        public Guid DashboardOptionId { get; set; }
        public DashboardOptionQueryDto DashboardOption { get; set; }

        public string Layout { get; set; }

        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
