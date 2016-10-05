using System;

namespace Hach.Fusion.FFCO.Dtos.Dashboards
{
    /// <summary>
    /// Data Transfer Object (DTO) for dashboard option entities used with query controller commands.
    /// </summary>
    public class DashboardOptionQueryDto : DashboardOptionBaseDto
    {
        public Guid TenantId { get; set; }
        public TenantDto Tenant { get; set; }

        public string Options { get; set; }

        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
