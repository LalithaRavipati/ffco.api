using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Entities;
// ReSharper disable InconsistentNaming

namespace Hach.Fusion.FFCO.Business.Database
{
    /// <summary>
    /// The database context for FFStorage.
    /// </summary>
    public class DataContext : DbContext
    {
        private const string Schema_ff = "ff";
        private const string Schema_dbo = "dbo";
        private const string Schema_foart = "foart";

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

        /// <summary>
        /// Gets or sets the DbSet containing <see cref="Location"/> entities.
        /// </summary>
        public DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the DbSet containing <see cref="LocationType"/> entities.
        /// </summary>
        public DbSet<LocationType> LocationTypes { get; set; }


        public DbSet<Tenant> Tenants { get; set; }



        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureSchemas(modelBuilder);

            modelBuilder.Conventions.Add(new ForeignKeyNamingConvention());

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Configure table schemas.
        /// </summary>
        private static void ConfigureSchemas(DbModelBuilder modelBuilder)
        {
            // Set default schema for tables.               
            modelBuilder.HasDefaultSchema(Schema_dbo);

            // Set ff schema for foundation tables.
            modelBuilder.Entity<LocationType>().ToTable("LocationTypes", Schema_ff);
            modelBuilder.Entity<TenantProductOffering>().ToTable("TenantProductOfferings")
                .HasKey(t => new {t.TenantId, t.ProductOfferingId});
        }
    }
}
