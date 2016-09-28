using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for parameter entities used with query controller commands.
    /// </summary>
    public class ParameterDto : FFDto<Guid>
    {
        public string I18NKeyName { get; set; }

        public Guid ParameterTypeId { get; set; }

        public Guid BaseUnitTypeId { get; set; }

        public Guid? BaseChemicalFormTypeId { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
