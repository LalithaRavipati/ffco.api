using System;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Dtos.Dashboards
{
    /// <summary>
    /// Data Transfer Object (DTO) for dashboard entities used with query controller commands.
    /// </summary>
    public class DashboardQueryDto : DashboardBaseDto
    {
        public Guid OwnerUserId { get; set; }
        public virtual User OwnerUser { get; set; }

        public Guid TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        public Guid DashboardOptionId { get; set; }
        //public virtual DashboardOptionDto DashboardOption { get; set; }

        public string Layout { get; set; }

        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
