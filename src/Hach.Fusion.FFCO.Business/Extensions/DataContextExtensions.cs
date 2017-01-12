using System;
using System.Linq;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Core.Entities;

namespace Hach.Fusion.FFCO.Business.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DataContext"/>.
    /// </summary>
    public static class DataContextExtensions
    {
        /// <summary>
        /// Returns all tenants that the user has access to.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="userId">Identifies the user.</param>
        /// <returns>Queryable list of tenants.</returns>
        public static IQueryable<Tenant> GetTenantsForUser(this DataContext context, Guid userId)
        {
            return (
                from t in context.Tenants
                where t.Users.Any(x => x.Id == userId)
                select t)
                .Distinct();
        }

        /// <summary>
        /// Returns all dashboards that are in any of the tenants that the user has access to.
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userId">Identifies the user.</param>
        /// <returns>Queryable list of dashboards.</returns>
        public static IQueryable<Dashboard> GetDashboardsForUser(this DataContext context, Guid userId)
        {
            return (
                from d in context.Dashboards
                where d.Tenant.Users.Any(x => x.Id == userId)
                select d)
                .Distinct();
        }

        /// <summary>
        /// Returns all dashboard options that are in any of the tenants that the user has access to.
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userId">Identifies the user.</param>
        /// <returns>Queryable list of dashboard options.</returns>
        public static IQueryable<DashboardOption> GetDashboardOptionsForUser(this DataContext context, Guid userId)
        {
            return (
                from d in context.DashboardOptions
                where d.Tenant.Users.Any(x => x.Id == userId)
                select d)
                .Distinct();
        }

        /// <summary>
        /// Returns all location log entries that are in any of the tenants that
        /// the user has access to.
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userId">Identifies the user.</param>
        /// <returns>Queryable list of location log entries.</returns>
        public static IQueryable<LocationLogEntry> GetLocationLogEntriesForUser(this DataContext context, Guid userId)
        {
            return
                from le in context.LocationLogEntries
                join potl in context.ProductOfferingTenantLocations on le.LocationId equals potl.LocationId
                where potl.Tenant.Users.Any(x => x.Id == userId)
                select le;
        }

        /// <summary>
        /// Returns the InAppMessages for the user.
        /// </summary>
        /// <param name="context">Database Context</param>
        /// <param name="userId">UserId GUID to get the InAppMessages for</param>
        /// <returns>Queryable list of InAppMessages </returns>
        public static IQueryable<InAppMessage> GetInAppMessagesForUser(this DataContext context, Guid userId)
        {
            return
                from msg in context.InAppMessages
                where msg.UserId == userId
                select msg;
        }

        /// <summary>
        /// Return locations the user is associated through its tenant(s)
        /// </summary>
        /// <param name="context">Database Context</param>
        /// <param name="userId">UserId</param>
        /// <returns></returns>
        public static IQueryable<Location> GetLocationsForUser(this DataContext context, Guid userId)
        {
            var locations = from location in context.Locations
                    join locTree in context.LocationTree on location.Id equals locTree.LocationId
                    where locTree.Tenant.Users.Any(u => u.Id == userId)
                    select location;

            return locations;
        }
    }
}