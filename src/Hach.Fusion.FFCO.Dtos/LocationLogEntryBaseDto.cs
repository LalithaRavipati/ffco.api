using System;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Dtos
{
    /// <summary>
    /// Base class for both command and query Location Log Entry Data Transfer Objects (DTOs).
    /// </summary>
    public class LocationLogEntryBaseDto : FFDto<Guid>
    {
        public Guid LocationId { get; set; }
        public string LogEntry { get; set; }
        public DateTimeOffset? TimeStamp { get; set; }
    }
}