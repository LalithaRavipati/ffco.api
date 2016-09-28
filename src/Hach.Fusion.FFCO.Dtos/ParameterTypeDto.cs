using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for ParameterType entities used with query controller commands.
    /// </summary>
    public class ParameterTypeDto : FFDto<Guid>
    {
        public string I18NKeyName { get; set; }
    }
}
