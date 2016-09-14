using System;
using Hach.Fusion.FFCO.Business.Tests.Extensions;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Tests.Data
{
    public static partial class SeedData
    {
        public static class LocationTypes
        {
            public static LocationType Plant =>
                new LocationType
                {
                    Id = Guid.Parse("66b8a397-96ae-4c6b-b1f1-9f28a823a8e6"),
                    I18NKeyName = "Plant"
                }.InitializeAuditFields();

            public static LocationType Site =>
                new LocationType
                {
                    Id = Guid.Parse("599292bf-44c7-4ef5-8624-04976a309577"),
                    I18NKeyName = "Site"
                }.InitializeAuditFields();

            public static LocationType MeasurementSite =>
                new LocationType
                {
                    Id = Guid.Parse("8d780f69-c4bb-4f2e-88e4-81251a5bfd37"),
                    I18NKeyName = "MeasurementSite"
                }.InitializeAuditFields();
        }
    }
}
