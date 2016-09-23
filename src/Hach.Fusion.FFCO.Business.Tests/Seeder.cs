using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Tests.Data;

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

            //SeedLocations(context);
            SeedParameters(context);
        }

        private static void DeleteAllExistingTestData(DataContext context)
        {
            context.Database.ExecuteSqlCommand("DELETE dbo.Locations");

            context.Database.ExecuteSqlCommand("DELETE ff.Parameters");
            context.Database.ExecuteSqlCommand("DELETE ff.LocationTypes");
            context.Database.ExecuteSqlCommand("DELETE ff.UnitTypes");
            context.Database.ExecuteSqlCommand("DELETE ff.ChemicalFormTypes");
            context.Database.ExecuteSqlCommand("DELETE ff.ParameterTypes");

            context.SaveChanges();
        }

        private static void SeedLocationTypes(DataContext context)
        {
            context.LocationTypes.Add(SeedData.LocationTypes.Plant);
            context.LocationTypes.Add(SeedData.LocationTypes.Site);
            context.LocationTypes.Add(SeedData.LocationTypes.MeasurementSite);

            context.SaveChanges();
        }

        private static void SeedLocations(DataContext context)
        {
            context.Locations.Add(SeedData.Locations.Location_1);
            context.Locations.Add(SeedData.Locations.Location_1A);
            context.Locations.Add(SeedData.Locations.Location_2);

            context.SaveChanges();
        }

        private static void SeedUnitTypes(DataContext context)
        {
            context.UnitTypes.Add(SeedData.UnitTypes.GallonsPerMinute);
            context.UnitTypes.Add(SeedData.UnitTypes.pH);

            context.SaveChanges();
        }

        private static void SeedParameterTypes(DataContext context)
        {
            context.ParameterTypes.Add(SeedData.ParameterTypes.Chemical);
            context.ParameterTypes.Add(SeedData.ParameterTypes.Sensed);

            context.SaveChanges();
        }

        private static void SeedParameters(DataContext context)
        {
            context.Parameters.Add(SeedData.Parameters.Flow);
            context.Parameters.Add(SeedData.Parameters.pH);

            context.SaveChanges();
        }
    }
}
