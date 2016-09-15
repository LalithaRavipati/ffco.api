using System;
using Hach.Fusion.FFCO.Business.Tests.Extensions;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Tests.Data
{
    public static partial class SeedData
    {
        public static class ParameterTypes
        {
            public static ParameterType Chemical =>
                new ParameterType
                {
                    Id = Guid.Parse("3A69CFEF-7735-4B24-A608-B6187F1676F5"),
                    I18NKeyName = "ParameterType_Chemical_Name"
                }.InitializeAuditFields();

            public static ParameterType Sensed =>
                new ParameterType
                {
                    Id = Guid.Parse("15301C9B-014E-4D6A-B9CF-D6EC4AB4B70F"),
                    I18NKeyName = "ParameterType_Sensed_Name"
                }.InitializeAuditFields();
        }
    }
}
