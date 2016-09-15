using System;
using Hach.Fusion.FFCO.Business.Tests.Extensions;
using Hach.Fusion.FFCO.Entities;
// ReSharper disable InconsistentNaming

namespace Hach.Fusion.FFCO.Business.Tests.Data
{
    public static partial class SeedData
    {
        public static class Parameters
        {
            public static Parameter Flow =>
                new Parameter
                {
                    Id = Guid.Parse("65B1168D-310E-47C6-BA0B-BA7D5F798998"),
                    I18NKeyName = "Parameter_Flow_Name",
                    ParameterTypeId = ParameterTypes.Sensed.Id,
                    IsDeleted = false,
                    BaseUnitTypeId = UnitTypes.GallonsPerMinute.Id,
                    BaseChemicalFormTypeId = null
                }.InitializeAuditFields();

            public static Parameter pH =>
                new Parameter
                {
                    Id = Guid.Parse("26C6C9EA-4348-495D-8980-8DF546B3B1A1"),
                    I18NKeyName = "Parameter_pH_Name",
                    ParameterTypeId = ParameterTypes.Sensed.Id,
                    IsDeleted = false,
                    BaseUnitTypeId = UnitTypes.pH.Id,
                    BaseChemicalFormTypeId = null
                }.InitializeAuditFields();
        }
    }
}
