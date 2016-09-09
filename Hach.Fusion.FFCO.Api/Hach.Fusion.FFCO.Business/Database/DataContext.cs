using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.DynamicFilters;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Database
{
    /// <summary>
    /// The database context for ASP.NET Identity.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class DataContext : DbContext
    {
        /// <summary>
        /// Name of the dynamic filter for soft deletes.
        /// </summary>
        private const string IsDeletedFilter = "IsDeleted";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DataContext(string connectionString)
            : base(connectionString)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataContext"/> class.
        /// </summary>
        public DataContext()
        { }

        /// <summary>
        /// The Locations DbSet.
        /// </summary>
        public virtual IDbSet<Location> Locations { get; set; }

        /// <summary>
        /// Provides customization when creating the database.
        /// </summary>
        /// <param name="modelBuilder"><see cref="DbModelBuilder"/></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure dynamic filter for IsDeleted:
            // See: https://github.com/jcachat/EntityFramework.DynamicFilters
            modelBuilder.Filter(IsDeletedFilter, (ISoftDeletableEntity e) => e.IsDeleted, false);
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
        public override async Task<int> SaveChangesAsync()
        {
            SoftDeleteSaveChanges();

            return await base.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while 
        /// waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result 
        /// contains the number of objects written to the underlying database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SoftDeleteSaveChanges();

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Handles soft deletes for entities that implement <see cref="ISoftDeletableEntity"/>.
        /// Some soft delete implementation from <see cref="http://putshello.wordpress.com/2014/08/20/entity-framework-soft-deletes-are-easy/"/>
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

