using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Dtos.Dashboards;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DataContext"/>.
    /// </summary>
    public static class DataContextExtensions
    { 
        /// <summary>
        /// Returns a task that gets all tenants that the user has access to.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="userId">Identifies the user.</param>
        /// <returns>Task that returns the results.</returns>
        public static Task<IQueryable<Tenant>> GetTenantsForUser(this DataContext context, Guid userId)
        {
            return Task.Run(() => (
                from t in context.Tenants
                where t.Users.Any(x => x.Id == userId)
                select t)
                .Distinct());
        }

        /// <summary>
        /// Returns a task that gets all dashboards that are in any of the tenants that
        /// the user has access to.
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userId">Identifies the user.</param>
        /// <returns>Task that returns the results.</returns>
        public static Task<IQueryable<Dashboard>> GetDashboardsForUser(this DataContext context, Guid userId)
        {
            return Task.Run(() => (
                from d in context.Dashboards
                where d.Tenant.Users.Any(x => x.Id == userId)
                select d)
                .Distinct());
        }

        /// <summary>
        /// Returns a task that gets all dashboard options that are in any of the tenants that
        /// the user has access to.
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userId">Identifies the user.</param>
        /// <returns>Task that returns the results.</returns>
        public static Task<IQueryable<DashboardOption>> GetDashboardOptionsForUser(this DataContext context, Guid userId)
        {
            return Task.Run(() => (
                from d in context.DashboardOptions
                where d.Tenant.Users.Any(x => x.Id == userId)
                select d)
                .Distinct());
        }
    }
}
