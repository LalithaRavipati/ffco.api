using System;
using Hach.Fusion.FFCO.Entities;
using Hach.Fusion.FFCO.Entities.Extensions;
using Hach.Fusion.FFCO.Entities.Seed;

namespace Hach.Fusion.FFCO.Business.Tests.SeedData
{
    public static partial class SeedData
    {
        public static class LocationLogEntries
        {
            public static LocationLogEntry Plant1Log1 =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("44E0C497-3A2C-4A89-99BD-F7B14C1A9187"),
                    LocationId = Data.Locations.Plant_01.Id,
                    LogEntry = "Log 1 for Plant 1"
                }.InitializeAuditFields();

            public static LocationLogEntry Plant1Log2 =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("44E0C497-3A2C-4A89-99BD-F7B14C1A9187"),
                    LocationId = Data.Locations.Plant_01.Id,
                    LogEntry = "Log 2 for Plant 1"
                }.InitializeAuditFields();

            public static LocationLogEntry PrimaryTreatmentLog1 =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("44E0C497-3A2C-4A89-99BD-F7B14C1A9187"),
                    LocationId = Data.Locations.Process_PrimaryTreatment.Id,
                    LogEntry = "Log 1 for Primary Treatment"
                }.InitializeAuditFields();


        }
    }
}
