namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for Location Log Entry entities used with create and update controller commands.
    /// </summary>
    /// <remarks>
    /// This class doesn't provide state or actions. It is needed in order for OData
    /// to function properly. OData controllers must be associated with one and only one entity
    /// type. However, different entity types are allowed if they derive from that one entity.
    /// </remarks>
    public class LocationLogEntryCommandDto : LocationBaseDto
    {
    }
}