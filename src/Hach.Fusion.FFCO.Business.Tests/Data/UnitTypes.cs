using System;
using Hach.Fusion.FFCO.Business.Tests.Extensions;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Tests.Data
{
    public static partial class SeedData
    {
        public static class UnitTypes
        {
            public static UnitType GallonsPerMinute =>
                new UnitType
                {
                    Id = Guid.Parse("030ECE27-9BB1-401A-87BF-2D75657D23BE"),
                    I18NKeyName = "UnitType_GallonsPerMinute_Name"
                }.InitializeAuditFields();

            public static UnitType pH =>
                new UnitType
                {
                    Id = Guid.Parse("74BFEEA6-5680-44FD-ADAB-11D3919FAD60"),
                    I18NKeyName = "UnitType_pH_Name"
                }.InitializeAuditFields();
        }
    }
}
