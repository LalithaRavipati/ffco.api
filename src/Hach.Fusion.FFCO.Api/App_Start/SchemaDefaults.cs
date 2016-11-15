using System;
using Hach.Fusion.FFCO.Core.Dtos;
using Swashbuckle.Swagger;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Schema filter for swagger documentation.
    /// </summary>
    public class SchemaDefaults : ISchemaFilter
    {
        /// <inheritdoc />
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type == typeof(LocationCommandDto))
            {
                schema.@default = new
                {
                    name = "Location Name",
                    locationTypeId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                    parentId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                    sortOrder = 0,
                    point = new
                    {
                        x = -105.9078,
                        y = 39.67434,
                        spatialReference = new
                        {
                            wkid = 4326
                        }
                    }
                };
            }
        }
    }
}