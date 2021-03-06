﻿using System;
using System.Collections.Generic;
using Hach.Fusion.Core.Dtos;
using Microsoft.OData.Core;
using Swashbuckle.Swagger;
using Hach.Fusion.Data.Dtos;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Schema filter for swagger documentation.
    /// </summary>
    public class SchemaDefaults : ISchemaFilter
    {
        /// <summary>
        /// Dictionary of model examples indexed by type.
        /// </summary>
        /// <remarks>
        /// Most examples are currently anonymous objects. A story for basing them on "real" objects has been
        /// created but not scheduled.
        /// </remarks>
        private static IDictionary<Type, object> Examples { get; }

        /// <summary>
        /// Static constructor for the <see cref="SchemaDefaults"/> class.
        /// </summary>
        static SchemaDefaults()
        {
            Examples = new Dictionary<Type, object>();

            BuildGlobalExamples();

            BuildLocationExamples();
            BuildParameterTypeExamples();
            BuildInAppMessageExamples();
            BuildDashboardExamples();
            BuildDashboardOptionExamples();
            BuildNotificationExamples();
        }

        /// <inheritdoc />
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (Examples.ContainsKey(type))
                schema.@default = Examples[type];
        }

        /// <summary>
        /// Builds model examples that are not specific to any particular controller for Swagger documentation.
        /// </summary>
        private static void BuildGlobalExamples()
        {
            Examples[typeof(ODataError)] = new
            {
                errorCode = "Fusion error code (e.g., FFERR-###)",
                message = "Error message"
            };

            Examples[typeof(SwaggerResponseInternalServerError)] = new
            {
                error = new
                {
                    code = "",
                    message = "An error has occurred."
                }
            };

            Examples[typeof(SwaggerResponseNotFound)] = new
            {
                errorCode = "FFERR-100",
                message = "Entity not found"
            };


            Examples[typeof(SwaggerResponseUnauthorized)] = new
            {
                error = new
                {
                    code = "",
                    message = "Authorization has been denied for this request."
                }
            };
        }

        /// <summary>
        /// Builds Location model examples for Swagger documentation.
        /// </summary>
        private static void BuildLocationExamples()
        {
            Examples[typeof(LocationQueryDto)] = new
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

        /// <summary>
        /// Builds Parameter Type model examples for Swagger documentation.
        /// </summary>
        private static void BuildParameterTypeExamples()
        {
            Examples[typeof(ParameterTypeQueryDto)] = new
            {
                id = Guid.Parse("768CC5ED-E6D6-4394-A9B6-0817C02506E8"),
                i18NKeyName = "ParameterType_Chemical",
                createdById = Guid.Parse("20A46218-57E4-4A28-BDF5-6B40A7BF6928"),
                createdOn = "2016-11-07T18:53:48.116Z",
                modifiedById = Guid.Parse("20A46218-57E4-4A28-BDF5-6B40A7BF6928"),
                modifiedOn = "2016-11-07T18:53:48.116Z"
            };
        }


        /// <summary>
        /// Builds the example data for InAppMessages for Swagger Documentation
        /// </summary>
        private static void BuildInAppMessageExamples()
        {
            Examples[typeof(InAppMessageQueryDto)] = new
            {
                body = "The is the body of the message",
                subject = "This is the subject of the message",
                userId = Guid.Parse("768CC5ED-E6D6-4394-A9B6-0817C02506E8"),
                isRead = false,
                isTrash = false,
                messageTypeId = Guid.Parse("768CC5ED-E6D6-4394-A9B6-0817C02506E8"),
                dateRead = "2016-11-07T18:53:48.116Z",
                dateReceived = "2016-11-07T18:53:48.116Z",
                dateTimeSent = "2016-11-07T18:53:48.116Z",
                senderId = Guid.Parse("768CC5ED-E6D6-4394-A9B6-0817C02506E8"),
                createdById = Guid.Parse("20A46218-57E4-4A28-BDF5-6B40A7BF6928"),
                createdOn = "2016-11-07T18:53:48.116Z",
                modifiedById = Guid.Parse("20A46218-57E4-4A28-BDF5-6B40A7BF6928"),
                modifiedOn = "2016-11-07T18:53:48.116Z"
            };

            Examples[typeof(InAppMessageQueryDto)] = new
            {
                isRead = false,
                isTrash = false
            };
        }

        /// <summary>
        /// Builds the example data for Dashboards for Swagger Documentation
        /// </summary>
        private static void BuildDashboardExamples()
        {
            Examples[typeof(DashboardQueryDto)] = new
            {
                dashboardOptionId = Guid.Parse("768CC5ED-0BAD-F00D-A9B6-0817C02506E8"),
                isPrivate = true,
                layout = "Layout1",
                name = "DashboardName",
                tenantId = Guid.Parse("879CC5ED-DEAD-BEEF-A9B6-0817C02506F9")
            };
        }

        /// <summary>
        /// Builds the example data for DashboardOptions for Swagger Documentation
        /// </summary>
        private static void BuildDashboardOptionExamples()
        {
            Examples[typeof(DashboardOptionQueryDto)] = new
            {
                options = "DashboardOption.1",
                tenantId = Guid.Parse("879CC5ED-DEAD-BEEF-A9B6-0817C02506F9")
            };
        }

        /// <summary>
        /// Builds the example data for Notifications for Swagger Documentation
        /// </summary>
        private static void BuildNotificationExamples()
        {
            Examples[typeof(NotificationDto)] = new
            {
                broadcastAll = false,
                groupName = "GroupName",
                message = "Message"
            };
        }
    }
}
