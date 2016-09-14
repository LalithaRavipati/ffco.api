using System;
using Hach.Fusion.FFCO.Business.Tests.Data;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Tests.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="BaseEntity"/>.
    /// </summary>
    public static class BaseEntityExtensions
    {
        /// <summary>
        /// Allows seed data to be created without setting the audit fields every time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T InitializeAuditFields<T>(this T entity) where T : BaseEntity
        {
            entity.CreatedById = SeedData.Users.Admin.Id;
            entity.ModifiedById = SeedData.Users.Admin.Id;
            entity.CreatedOn = DateTime.UtcNow;
            entity.ModifiedOn = DateTime.UtcNow;
            return entity;
        }
    }
}
