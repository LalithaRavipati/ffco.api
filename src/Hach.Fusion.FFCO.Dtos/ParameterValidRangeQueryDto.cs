using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for Parameter Valid Range entities used with query controller commands.
    /// </summary>
    public class ParameterValidRangeQueryDto : FFDto<Guid>
    {
        public Guid ParameterId { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }

        public ParameterDto Parameter { get; set; }

        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
