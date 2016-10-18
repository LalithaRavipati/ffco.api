using System;
using Hach.Fusion.Core.Dtos;
using Hach.Fusion.FFCO.Entities;

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

        // Expandable Properties
        public virtual ParameterTypeDto ParameterType { get; set; }
        public virtual UnitTypeQueryDto BaseUnitType { get; set; }
        public virtual ChemicalFormTypeQueryDto BaseChemicalFormType { get; set; }
    }
}
