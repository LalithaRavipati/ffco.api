using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Database
{
    /// <summary>
    /// The database context for FFStorage.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class DataContext : DbContext
    {
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
            // Read the DateTimeKindAttributes and set them during entity materialization when appopriate.
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized +=
                (sender, e) => DateTimeKindAttribute.Apply(e.Entity);
        }

        /// <summary>
        /// Gets or sets the DbSet containing <see cref="Locations"/> entities.
        /// </summary>
        public DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the DbSet containing <see cref="Parameters"/> entities.
        /// </summary>
        public DbSet<Parameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the DbSet containing <see cref="ParameterType"/> entities.
        /// </summary>
        public DbSet<ParameterType> ParameterTypes { get; set; }

        /// <summary>
        /// Gets or sets the DbSet containing <see cref="UnitType"/> entities.
        /// </summary>
        public DbSet<UnitType> UnitTypes { get; set; }

        /// <summary>
        /// Gets or sets the DbSet containing <see cref="ChemicalFormTypes"/> entities.
        /// </summary>
        public DbSet<ChemicalFormType> ChemicalFormTypes { get; set; }
    }
}
