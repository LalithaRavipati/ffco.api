using System;

namespace Hach.Fusion.FFCO.Dtos.LimitTypes
{
    /// <summary>
    /// Data Transfer Object (DTO) for LimitType entities used with query controller commands.
    /// </summary>
    public class LimitTypeQueryDto : LimitTypeBaseDto
    {
        public string I18NKeyName { get; set; }
        public int Severity { get; set; }
        public int Polarity { get; set; }

        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
