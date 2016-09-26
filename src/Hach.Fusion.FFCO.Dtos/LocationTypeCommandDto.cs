using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for location entities used with create and update controller commands.
    /// </summary>
    public class LocationTypeCommandDto : FFDto<Guid>
    {
        public string I18NKeyName { get; set; }
    }
}
