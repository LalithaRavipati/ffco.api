using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Entities;
using Hach.Fusion.FFCO.Entities.Hach.Fusion.FFCO.Entities;

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

        public DbSet<ProductOffering> ProductOfferings { get; set; }

        public DbSet<ProductOfferingTenantLocation> ProductOfferingTenantLocations { get; set; }



        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureSchemas(modelBuilder);

            modelBuilder.Conventions.Add(new ForeignKeyNamingConvention());

            //modelBuilder.Entity<Tenant>().ToTable("Tenants")
            //    .HasKey(t => t.Id);

            //modelBuilder.Entity<TenantProductOffering>()
            //    .HasKey(t => new { t.TenantId, t.ProductOfferingId });

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

            // Set ff schema for foundation tables.
            modelBuilder.Entity<LocationType>().ToTable("LocationTypes", Schema_ff);
        }
    }
}
