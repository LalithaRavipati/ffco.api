namespace Hach.Fusion.FFCO.Dtos.LimitTypes
{
    /// <summary>
    /// Data Transfer Object (DTO) for LimitType entities used with create and update controller commands.
    /// </summary>
    public class LimitTypeCommandDto : LimitTypeBaseDto
    {
        public string I18NKeyName { get; set; }
        public int Severity { get; set; }
        public int Polarity { get; set; }
    }
}
