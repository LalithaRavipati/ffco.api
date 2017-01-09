﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Core.Entities;

// ReSharper disable InconsistentNaming

namespace Hach.Fusion.FFCO.Business.Database
{
    /// <summary>
    /// The database context for the FFCO Api
    /// </summary>
    public class DataContext : DbContext
    {
        private const string Schema_dbo = "dbo";

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
        public DbSet<LocationLogEntry> LocationLogEntries { get; set; }
        public DbSet<UnitType> UnitTypes { get; set; }
        public DbSet<UnitTypeGroup> UnitTypeGroups { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ProductOffering> ProductOfferings { get; set; }
        public DbSet<ProductOfferingTenantLocation> ProductOfferingTenantLocations { get; set; }
        public DbSet<ParameterType> ParameterTypes { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<ParameterValidRange> ParameterValidRanges { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DashboardOption> DashboardOptions { get; set; }
        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<LimitType> LimitTypes { get; set; }
        public DbSet<ChemicalFormType> ChemicalFormTypes { get; set; }
        public DbSet<InAppMessage> InAppMessages { get; set; }
        public DbSet<MessageType> MessageTypes { get; set; }



        // Added View based on the following stack overflow post
        // http://stackoverflow.com/questions/7461265/how-to-use-views-in-code-first-entity-framework
        public DbSet<LocationTreeNode> LocationTree { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureSchemas(modelBuilder);

            modelBuilder.Entity<ProductOfferingTenantLocation>()
                .ToTable("ProductOfferingsTenantsLocations")
                .HasKey(e => new { e.ProductOfferingId, e.TenantId, e.LocationId });

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


            modelBuilder.Entity<Tenant>()
                .HasMany(e => e.ProductOfferings)
                .WithMany(e => e.Tenants)
                .Map(e =>
                {
                    e.ToTable("TenantProductOfferings")
                    .MapLeftKey("Tenant_Id")
                    .MapRightKey("ProductOffering_Id");
                });

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Configure table schemas.
        /// </summary>
        private static void ConfigureSchemas(DbModelBuilder modelBuilder)
        {
            // Set default schema for tables.               
            modelBuilder.HasDefaultSchema(Schema_dbo);

            modelBuilder.Configurations.Add(new LocationTreeNodeConfiguration());

        }
    }
}