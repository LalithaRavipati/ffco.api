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
            SeedLocations(context);
        }

        private static void DeleteAllExistingTestData(DataContext context)
        {
            context.Database.ExecuteSqlCommand("DELETE dbo.ProductOfferingsTenantsLocations");
            context.Database.ExecuteSqlCommand("DELETE dbo.Locations");
            context.Database.ExecuteSqlCommand("DELETE dbo.LocationTypes");

            context.SaveChanges();
        }

        private static void SeedLocationTypes(DataContext context)
        {
            context.LocationTypes.Add(Data.LocationTypes.Plant);
            context.LocationTypes.Add(Data.LocationTypes.Process);
            context.LocationTypes.Add(Data.LocationTypes.SamplingSite);

            context.SaveChanges();
        }

        private static void SeedLocations(DataContext context)
        {
            context.Locations.Add(Data.Locations.Location_Plant_01);
            context.Locations.Add(Data.Locations.Location_Process_Preliminary);
            context.Locations.Add(Data.Locations.Location_SamplingSite_Grit);
            context.Locations.Add(Data.Locations.Location_SamplingSite_Screenings);
            context.Locations.Add(Data.Locations.Location_SamplingSite_Chemical);

            context.Locations.Add(Data.Locations.Location_Process_Influent);
            context.Locations.Add(Data.Locations.Location_SamplingSite_Influent_InfluentCombined);
            context.Locations.Add(Data.Locations.Location_SamplingSite_Influent_HauledWasted);

            context.SaveChanges();
        }
    }
}
