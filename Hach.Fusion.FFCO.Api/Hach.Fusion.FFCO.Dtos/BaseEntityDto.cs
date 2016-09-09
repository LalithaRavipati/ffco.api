using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Base class for Data Transfer Objects (DTOs) that support entities that are derived
    /// from Base Entity. 
    /// </summary>
    public class BaseEntityDto : FFDto<Guid>
    {
        public Guid CreatedById { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
