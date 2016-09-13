using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Navigation routing convention to support property path segments in urls.
    /// </summary>
    /// <remarks>
    /// Note that this version of the property routing convention handles the case
    /// where the routing path template contains "unresolved" for the property name.
    /// This can occur when the OData Entity Data Model uses property names that are derived
    /// an entity set.
    /// </remarks>
    public class CustomPropertyRoutingConvention : NavigationSourceRoutingConvention
    {
        private const string ActionName = "GetProperty";

        /// <inheritdoc />
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath == null || controllerContext == null || actionMap == null)
            {
                return null;
            }

            if (odataPath.PathTemplate != "~/entityset/key/property" &&
                odataPath.PathTemplate != "~/entityset/key/unresolved" &&
                odataPath.PathTemplate != "~/entityset/key/cast/property" &&
                odataPath.PathTemplate != "~/singleton/property" &&
                odataPath.PathTemplate != "~/singleton/cast/property")
            {
                return null;
            }

            var propertyName = GetPropertyName(odataPath.Segments);

            if (propertyName == null)
                return null;

            var actionName = FindMatchingAction(actionMap, ActionName);

            if (actionName == null)
            {
                return null;
            }

            if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
            {
                var keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
                // ReSharper disable once PossibleNullReferenceException
                controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
            }

            controllerContext.RouteData.Values["propertyName"] = propertyName;

            return actionName;
        }

        /// <summary>
        /// Retrieves the property name from the routing segments.
        /// </summary>
        /// <param name="segments">Routing segments resulting from a parsed request.</param>
        /// <returns>The property name from the routing segments or null if the property name is not available.</returns>
        private static string GetPropertyName(IList<ODataPathSegment> segments)
        {
            var propertySegment = segments[segments.Count - 1] as PropertyAccessPathSegment;

            if (propertySegment == null)
            {
                var unresolvedSegment = segments[segments.Count - 1] as UnresolvedPathSegment;
                return unresolvedSegment?.SegmentValue;
            }

            return propertySegment.PropertyName;
        }

        /// <summary>
        /// Finds a controller action.
        /// </summary>
        /// <param name="actionMap">Map containing all actions.</param>
        /// <param name="targetActionName">Name of the action to find.</param>
        /// <returns>Name of action.</returns>
        public static string FindMatchingAction(ILookup<string, HttpActionDescriptor> actionMap,
            string targetActionName)
        {
            return actionMap.Contains(targetActionName) ? targetActionName : null;
        }
    }
}