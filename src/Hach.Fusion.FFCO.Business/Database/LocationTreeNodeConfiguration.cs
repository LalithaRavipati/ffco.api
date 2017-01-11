using Hach.Fusion.FFCO.Core.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Hach.Fusion.FFCO.Business.Database
{
    class LocationTreeNodeConfiguration : EntityTypeConfiguration<LocationTreeNode>
    {
        // Adding the view based on the below stack overflow question
        // http://stackoverflow.com/questions/7461265/how-to-use-views-in-code-first-entity-framework

        /// <summary>
        /// Configuration Class for the dbo.vw_LocationTree view code first entity
        /// </summary>
        public LocationTreeNodeConfiguration()
        {
            HasKey(t => t.LocationId);
            ToTable("vw_LocationTree");
        }
    }
}