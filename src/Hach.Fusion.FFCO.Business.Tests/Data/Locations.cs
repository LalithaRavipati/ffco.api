using System;
using Hach.Fusion.FFCO.Business.Tests.Extensions;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Tests.Data
{
    public static partial class SeedData
    {
        public static class Locations
        {
            public static Location Location_1 =>
                new Location
                {
                    Id = Guid.Parse("B73C185D-667F-4636-A245-AB7B8EAA9BDA"),

                    IsDeleted = false,
                    Name = "Location_1",
                    LocationTypeId = LocationTypes.Plant.Id,
                    ParentId = null
                }.InitializeAuditFields();

            public static Location Location_1A =>
                new Location
                {
                    Id = Guid.Parse("5E510C45-7BBE-47C6-80D1-F86D79F418E6"),

                    IsDeleted = false,
                    Name = "Location_1A",
                    LocationTypeId = LocationTypes.Plant.Id,
                    ParentId = Location_1.Id

                }.InitializeAuditFields();

            public static Location Location_2 =>
                new Location
                {
                    Id = Guid.Parse("DAFC161D-08B0-401A-9B29-33CB510F992C"),

                    IsDeleted = false,
                    Name = "Location_2",
                    LocationTypeId = LocationTypes.Plant.Id,
                    ParentId = null
                }.InitializeAuditFields();
        }
    }
}
