using System.Data.Entity;
using System.Linq;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Core.Entities;

namespace Hach.Fusion.FFCO.Business.Extensions
{
    /// <summary>
    /// Class containing extension methods for <see cref="IQueryable"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IQueryableExtensions
    {
        /// <summary>
        /// To simplify including common foreign key relationships
        /// </summary>
        /// <param name="context">The entity framework database context.</param>
        /// <returns>An IQueryable of <see cref="Location"/>.</returns>
        public static IQueryable<Location> ExpandedLocations(this DataContext context)
        {
            return context.Locations
                .Include(x => x.Parent)
                .Include(x => x.Locations)
                .Include(x => x.ProductOfferingTenantLocations);
        }
    }
}
