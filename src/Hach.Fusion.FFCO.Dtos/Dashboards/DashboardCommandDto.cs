using System;

namespace Hach.Fusion.FFCO.Dtos.Dashboards
{
    /// <summary>
    /// Data Transfer Object (DTO) for dashboard entities used with create and update controller commands.
    /// </summary>
    public class DashboardCommandDto : DashboardBaseDto
    {
        public Guid OwnerUserId { get; set; }
        public Guid TenantId { get; set; }
        public Guid DashboardOptionId { get; set; }
        public string Layout { get; set; }
    }
}
