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
            SeedLocations(context);
        }

        private static void DeleteAllExistingTestData(DataContext context)
        {
            context.Database.ExecuteSqlCommand("DELETE dbo.Locations");

            context.Database.ExecuteSqlCommand("DELETE ff.LocationTypes");

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
    }
}
