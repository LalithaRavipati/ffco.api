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
                    TenantId = Guid.Parse("F889DC4C-3A42-45C0-A34B-10E700562C5F"),
                    Name = "Location_1",
                    LocationTypeId = LocationTypes.Plant.Id,
                    ParentId = null
                }.InitializeAuditFields();

            public static Location Location_1A =>
                new Location
                {
                    Id = Guid.Parse("5E510C45-7BBE-47C6-80D1-F86D79F418E6"),

                    IsDeleted = false,
                    TenantId = Guid.Parse("F889DC4C-3A42-45C0-A34B-10E700562C5F"),
                    Name = "Location_1A",
                    LocationTypeId = LocationTypes.Plant.Id,
                    ParentId = Location_1.Id

                }.InitializeAuditFields();

            public static Location Location_2 =>
                new Location
                {
                    Id = Guid.Parse("DAFC161D-08B0-401A-9B29-33CB510F992C"),

                    IsDeleted = false,
                    TenantId = Guid.Parse("2FF165E3-1D53-4560-B697-4A10249A9D10"),
                    Name = "Location_2",
                    LocationTypeId = LocationTypes.Plant.Id,
                    ParentId = null
                }.InitializeAuditFields();
        }
    }
}
