using System;

namespace Hach.Fusion.FFCO.Dtos.Dashboards
{
    /// <summary>
    /// Data Transfer Object (DTO) for dashboard option entities used with create and update controller commands.
    /// </summary>
    public class DashboardOptionCommandDto
    {
        public Guid TenantId { get; set; }

        public string Options { get; set; }
    }
}
