using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Entities.Seed;

namespace Hach.Fusion.FFCO.Business.Tests
{
    public static class Seeder
    {
        public static void SeedWithTestData(DataContext context)
        {
            DeleteAllExistingTestData(context);

            SeedLocationTypes(context);
            SeedParameterTypes(context);
            SeedUnitTypes(context);
            SeedUnitTypeGroups(context);

            SeedLocations(context);
            SeedParameters(context);
            SeedProductOfferingTenantLocations(context);
        }

        private static void DeleteAllExistingTestData(DataContext context)
        {
            context.Database.ExecuteSqlCommand("DELETE dbo.ProductOfferingsTenantsLocations");
            context.Database.ExecuteSqlCommand("DELETE foart.Measurements");
            context.Database.ExecuteSqlCommand("DELETE foart.LocationParameters");
            context.Database.ExecuteSqlCommand("DELETE dbo.Locations");
            context.Database.ExecuteSqlCommand("DELETE dbo.ParameterValidRanges");
            context.Database.ExecuteSqlCommand("DELETE dbo.Parameters");

            context.Database.ExecuteSqlCommand("DELETE dbo.LocationTypes");
            context.Database.ExecuteSqlCommand("DELETE dbo.LocationTypes");
            context.Database.ExecuteSqlCommand("DELETE dbo.UnitTypeGroups");
            context.Database.ExecuteSqlCommand("DELETE dbo.UnitTypes");
            //context.Database.ExecuteSqlCommand("DELETE dbo.ChemicalFormTypes");
            context.Database.ExecuteSqlCommand("DELETE dbo.ParameterTypes");

            context.SaveChanges();
        }

        private static void SeedLocationTypes(DataContext context)
        {
            context.LocationTypes.Add(Data.LocationTypes.Plant);
            context.LocationTypes.Add(Data.LocationTypes.Process);
            context.LocationTypes.Add(Data.LocationTypes.SamplingSite);
            context.LocationTypes.Add(Data.LocationTypes.Distribution);

            context.SaveChanges();
        }

        private static void SeedLocations(DataContext context)
        {
            context.Locations.Add(Data.Locations.Plant_01);
            context.Locations.Add(Data.Locations.Process_Preliminary);
            context.Locations.Add(Data.Locations.Process_Influent);
            context.Locations.Add(Data.Locations.Process_PrimaryTreatment);
            context.Locations.Add(Data.Locations.Process_SecondaryTreatment);
            context.Locations.Add(Data.Locations.SamplingSite_Grit);
            context.Locations.Add(Data.Locations.SamplingSite_Screenings);
            context.Locations.Add(Data.Locations.SamplingSite_Chemical);
            context.Locations.Add(Data.Locations.SamplingSite_Influent_InfluentCombined);
            context.Locations.Add(Data.Locations.SamplingSite_Influent_HauledWasted);
            context.Locations.Add(Data.Locations.SamplingSite_Influent_Recycled);

            context.Locations.Add(Data.Locations.Test_SoftDeleted);
            context.Locations.Add(Data.Locations.Test_SoftDeletable);
            context.Locations.Add(Data.Locations.Test_Updateable);
            
            context.SaveChanges();
        }

        private static void SeedUnitTypeGroups(DataContext context)
        {
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.Volume);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.VolumeTime);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.AreaTime);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.Length);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.Mass);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.MassTime);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.Area);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.LengthTime);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.Time);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.pH);
            context.UnitTypeGroups.Add(Data.UnitTypeGroups.Temp);

            context.SaveChanges();
        }

        private static void SeedUnitTypes(DataContext context)
        {
            context.UnitTypes.Add(Data.UnitTypes.GrainsPerGallon);
            context.UnitTypes.Add(Data.UnitTypes.Millimeters);
            context.UnitTypes.Add(Data.UnitTypes.PartsPerMillion);
            context.UnitTypes.Add(Data.UnitTypes.MilligramsPerLiter);
            context.UnitTypes.Add(Data.UnitTypes.MillisiemensPerCentimeter);
            context.UnitTypes.Add(Data.UnitTypes.Centigrade);
            context.UnitTypes.Add(Data.UnitTypes.MillionGallonsPerDay);
            context.UnitTypes.Add(Data.UnitTypes.Inches);
            context.UnitTypes.Add(Data.UnitTypes.Millibar);
            context.UnitTypes.Add(Data.UnitTypes.MicrogramsPerLiter);
            context.UnitTypes.Add(Data.UnitTypes.PercentSaturation);
            context.UnitTypes.Add(Data.UnitTypes.GermanDegrees);
            context.UnitTypes.Add(Data.UnitTypes.InchesOfMercury);
            context.UnitTypes.Add(Data.UnitTypes.PartsPerBillion);
            context.UnitTypes.Add(Data.UnitTypes.Hectopascal);
            context.UnitTypes.Add(Data.UnitTypes.Fahrenheit);
            context.UnitTypes.Add(Data.UnitTypes.EnglishDegrees);
            context.UnitTypes.Add(Data.UnitTypes.FrenchDegrees);
            context.UnitTypes.Add(Data.UnitTypes.MicrosiemensPerCentimeter);
            context.UnitTypes.Add(Data.UnitTypes.Millivolts);
            context.UnitTypes.Add(Data.UnitTypes.Percent);
            context.UnitTypes.Add(Data.UnitTypes.pH);
            context.UnitTypes.Add(Data.UnitTypes.GallonsPerMinute);
            context.UnitTypes.Add(Data.UnitTypes.MillimetersOfMercury);

            context.SaveChanges();
        }

        private static void SeedParameterTypes(DataContext context)
        {
            context.ParameterTypes.Add(Data.ParameterTypes.Chemical);
            context.ParameterTypes.Add(Data.ParameterTypes.Sensed);

            context.SaveChanges();
        }

        private static void SeedParameters(DataContext context)
        {
            context.Parameters.Add(Data.Parameters.Flow);
            context.Parameters.Add(Data.Parameters.pH);

            context.SaveChanges();
        }

        private static void SeedProductOfferingTenantLocations(DataContext context)
        {
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_Plant1);
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_InfluentCombined);
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_InfluentRecycled);

            context.SaveChanges();
        }
    }
}
