using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Core.Seed;

namespace Hach.Fusion.FFCO.Business.Tests
{
    public static class Seeder
    {
        public static void SeedWithTestData(DataContext context)
        {
            DeleteAllExistingTestData(context);

            SeedLocationTypes(context);
            SeedParameterTypes(context);
            SeedUnitTypeGroups(context);
            SeedUnitTypes(context);
            SeedLimitTypes(context);

            SeedLocations(context);
            SeedProductOfferingTenantLocations(context);
            SeedDashboardOptions(context);
            SeedDashboards(context);
            SeedLocationLogEntries(context);
            SeedParameters(context);
            SeedParameterValidRanges(context);
            SeedChemicalTypes(context);
        }

        private static void DeleteAllExistingTestData(DataContext context)
        {
            context.Database.ExecuteSqlCommand("DELETE dbo.ProductOfferingsTenantsLocations");
            context.Database.ExecuteSqlCommand("DELETE foart.LocationParameterLimits");
            context.Database.ExecuteSqlCommand("DELETE foart.LocationParameterNotes");
            context.Database.ExecuteSqlCommand("DELETE foart.Measurements");
            context.Database.ExecuteSqlCommand("DELETE foart.MeasurementTransactions");
            context.Database.ExecuteSqlCommand("DELETE dbo.LocationLogEntries");
            context.Database.ExecuteSqlCommand("DELETE foart.LocationParameters");
            context.Database.ExecuteSqlCommand("DELETE dbo.Locations");
            context.Database.ExecuteSqlCommand("DELETE dbo.ParameterValidRanges");
            context.Database.ExecuteSqlCommand("DELETE dbo.Parameters");
            context.Database.ExecuteSqlCommand("DELETE dbo.Dashboards");
            context.Database.ExecuteSqlCommand("DELETE dbo.DashboardOptions");

            context.Database.ExecuteSqlCommand("DELETE dbo.LocationTypes");
            context.Database.ExecuteSqlCommand("DELETE dbo.UnitTypes");
            context.Database.ExecuteSqlCommand("DELETE dbo.UnitTypeGroups");
            context.Database.ExecuteSqlCommand("DELETE dbo.ParameterTypes");
            context.Database.ExecuteSqlCommand("DELETE dbo.ParameterValidRanges");
            context.Database.ExecuteSqlCommand("DELETE dbo.LimitTypes");
            context.Database.ExecuteSqlCommand("DELETE dbo.ChemicalFormTypes");

            context.SaveChanges();
        }

        private static void SeedChemicalTypes(DataContext context)
        {
            context.ChemicalFormTypes.Add(Data.ChemicalFormTypes.Alum);
            context.ChemicalFormTypes.Add(Data.ChemicalFormTypes.Caffeine);
            context.ChemicalFormTypes.Add(Data.ChemicalFormTypes.Ethanol);
            context.ChemicalFormTypes.Add(Data.ChemicalFormTypes.GalliumArsenide);
            context.ChemicalFormTypes.Add(Data.ChemicalFormTypes.Water);

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
            context.Locations.Add(Data.Locations.Plant_02);
            context.Locations.Add(Data.Locations.Plant_03);

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

            context.Locations.Add(Data.Locations.Test_Updateable);
            
            context.SaveChanges();
        }

        private static void SeedLocationLogEntries(DataContext context)
        {
            context.LocationLogEntries.Add(Data.LocationLogEntries.Plant1Log1);
            context.LocationLogEntries.Add(Data.LocationLogEntries.Plant1Log2);
            context.LocationLogEntries.Add(Data.LocationLogEntries.Plant2Log1);
            context.LocationLogEntries.Add(Data.LocationLogEntries.Plant3Log1);

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

        private static void SeedParameterValidRanges(DataContext context)
        {
            context.ParameterValidRanges.Add(Data.ParameterValidRanges.pH);
            context.ParameterValidRanges.Add(Data.ParameterValidRanges.FlowMinAndMax);
            context.ParameterValidRanges.Add(Data.ParameterValidRanges.FlowMinOnly);
            context.ParameterValidRanges.Add(Data.ParameterValidRanges.FlowMaxOnly);

            context.SaveChanges();
        }

        private static void SeedProductOfferingTenantLocations(DataContext context)
        {
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_Plant1);
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_Plant2);
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_Plant3);
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_InfluentCombined);
            context.ProductOfferingTenantLocations.Add(Data.ProductOfferingTenantLocations.FusionFoundation_HachFusion_InfluentRecycled);

            context.SaveChanges();
        }

        private static void SeedDashboardOptions(DataContext context)
        {
            context.DashboardOptions.Add(Data.DashboardOptions.DevTenant01_Options);
            context.DashboardOptions.Add(Data.DashboardOptions.DevTenant02_Options);

            context.SaveChanges();
        }

        private static void SeedDashboards(DataContext context)
        {
            context.Dashboards.Add(Data.Dashboards.tnt01user_Dashboard_1);
            context.Dashboards.Add(Data.Dashboards.tnt01user_Dashboard_2);
            context.Dashboards.Add(Data.Dashboards.tnt02user_Dashboard_3);
            context.Dashboards.Add(Data.Dashboards.tnt01and02user_Dashboard_4);
            context.Dashboards.Add(Data.Dashboards.tnt01and02user_Dashboard_5);
            context.Dashboards.Add(Data.Dashboards.Test_tnt01user_ToDelete);
            context.Dashboards.Add(Data.Dashboards.Test_tnt01user_ToUpdate);

            context.SaveChanges();
        }

        private static void SeedLimitTypes(DataContext context)
        {
            context.LimitTypes.Add(Data.LimitTypes.Under);
            context.LimitTypes.Add(Data.LimitTypes.Over);
            context.LimitTypes.Add(Data.LimitTypes.NearUnder);
            context.LimitTypes.Add(Data.LimitTypes.NearOver);
            context.LimitTypes.Add(Data.LimitTypes.ToDelete);

            context.SaveChanges();
        }
    }
}
