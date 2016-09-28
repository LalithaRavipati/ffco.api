using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.DynamicFilters;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Entities;
using Hach.Fusion.FFCO.Entities.Hach.Fusion.FFCO.Entities;

// ReSharper disable InconsistentNaming

namespace Hach.Fusion.FFCO.Business.Database
{
    /// <summary>
    /// The database context for the FFCO Api
    /// </summary>
    public class DataContext : DbContext
    {
        private const string Schema_dbo = "dbo";
        private const string IsDeletedFilter = "IsDeleted";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataContext()
        { }

        /// <summary>
        /// Constructor that accepts a database connection string.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DataContext(string connectionString) : base(connectionString)
        {
            System.Data.Entity.Database.SetInitializer<DataContext>(null);

            // Read the DateTimeKindAttributes and set them during entity materialization when appopriate.
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized +=
                (sender, e) => DateTimeKindAttribute.Apply(e.Entity);
        }

        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationType> LocationTypes { get; set; }
        public DbSet<UnitType> UnitTypes { get; set; }
        public DbSet<UnitTypeGroup> UnitTypeGroups { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ProductOffering> ProductOfferings { get; set; }
        public DbSet<ProductOfferingTenantLocation> ProductOfferingTenantLocations { get; set; }
        public DbSet<ParameterType> ParameterTypes { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureSchemas(modelBuilder);

            // modelBuilder.Conventions.Add(new ForeignKeyNamingConvention());

            // Configure dynamic filter for IsDeleted:
            // See: https://github.com/jcachat/EntityFramework.DynamicFilters
            modelBuilder.Filter(IsDeletedFilter, (ISoftDeletableEntity e) => e.IsDeleted, false);

            modelBuilder.Entity<ProductOfferingTenantLocation>()
                .ToTable("ProductOfferingsTenantsLocations")
                .HasKey(e => new {e.ProductOfferingId, e.TenantId, e.LocationId});

            modelBuilder.Entity<ProductOfferingTenantLocation>()
                .HasRequired(e => e.ProductOffering)
                .WithMany(e => e.ProductOfferingTenantLocations)
                .HasForeignKey(e => e.ProductOfferingId);

            modelBuilder.Entity<ProductOfferingTenantLocation>()
                .HasRequired(e => e.Tenant)
                .WithMany(e => e.ProductOfferingTenantLocations)
                .HasForeignKey(e => e.TenantId);

            modelBuilder.Entity<ProductOfferingTenantLocation>()
                .HasRequired(e => e.Location)
                .WithMany(e => e.ProductOfferingTenantLocations)
                .HasForeignKey(e => e.LocationId);

                        base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Configure table schemas.
        /// </summary>
        private static void ConfigureSchemas(DbModelBuilder modelBuilder)
        {
            // Set default schema for tables.               
            modelBuilder.HasDefaultSchema(Schema_dbo);            
        }


        #region Soft Delete Support

        /// <summary>
        /// Enables the IsDeleted dynamic filter.
        /// </summary>
        public void EnableIsDeletedFilter()
        {
            this.EnableFilter(IsDeletedFilter);
        }

        /// <summary>
        /// Disables the IsDeleted dynamic filter.
        /// </summary>
        public void DisableIsDeletedFilter()
        {
            this.DisableFilter(IsDeletedFilter);
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>The number of objects written to the underlying database.</returns>
        public override int SaveChanges()
        {
            SoftDeleteSaveChanges();

            return base.SaveChanges();
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation. The task result 
        /// contains the number of objects written to the underlying database.</returns>
        public override Task<int> SaveChangesAsync()
        {
            SoftDeleteSaveChanges();

            return base.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while 
        /// waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result 
        /// contains the number of objects written to the underlying database.</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SoftDeleteSaveChanges();

            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Handles soft deletes for entities that implement <see cref="ISoftDeletableEntity"/>.
        /// Some soft delete implementation from <see cref="https://putshello.wordpress.com/2014/08/20/entity-framework-soft-deletes-are-easy/"/>
        /// </summary>
        private void SoftDeleteSaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries().Where(s => s.State == EntityState.Deleted).Where(entry => entry.Entity is ISoftDeletableEntity))
            {
                SoftDelete(entry);
            }
        }

        /// <summary>
        /// Handles soft deletes for an entity that implement <see cref="ISoftDeletableEntity"/>.
        /// </summary>
        /// <param name="entry">The entity to be soft deleted.</param>
        private static void SoftDelete(DbEntityEntry entry)
        {
            entry.Property("IsDeleted").CurrentValue = true;            

            // prevent hard delete            
            entry.State = EntityState.Modified;
        }

        #endregion Soft Delete Support
    }
}
