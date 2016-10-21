using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for UnitType entities used with query controller commands.
    /// </summary>
    public class ChemicalFormTypeQueryDto : FFDto<Guid>
    {
        public string I18NKeyName { get; set; }

        public string Form { get; set; }

        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}