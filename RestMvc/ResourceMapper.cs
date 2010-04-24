using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestMvc
{
    /// <summary>
    /// Reflects on the attributes in TController to discover the routes
    /// to be added.  Each TController can support multiple resource types.
    /// This is quite common - typically an Index (list) and Show (entity)
    /// are supported by the same controller, as are edit and create forms,
    /// all of which have different resource types defined by different
    /// URI templates.
    /// </summary>
    /// <typeparam name="TController">The type of controller to add routes for</typeparam>
    public class ResourceMapper<TController> where TController : RestfulController
    {
        /// <summary>
        /// Maps all the routes provided by ResourceActionAttribute annotations.
        /// </summary>
        public virtual void MapSupportedMethods(ICollection<RouteBase> routes)
        {
            foreach (var action in typeof(TController).GetResourceActions())
            {
                var attribute = action.GetResourceActionAttribute();
                Map(routes, attribute.ResourceUri, Defaults(action.Name), attribute.HttpMethod);
                if (attribute.HttpMethod == "GET")
                    Map(routes, attribute.ResourceUri + ".{format}", Defaults(action.Name), attribute.HttpMethod);
            }
        }

        /// <summary>
        /// For every resource URI referenced in a ResourceActionAttribute,
        /// maps the HTTP methods _not_ supported at that URI to a method
        /// on the RestfulController that returns a 405 HTTP code.
        /// This does not include the HEAD, OPTIONS, or WebDAV methods.
        /// </summary>
        public virtual void MapUnsupportedMethods(RouteCollection routes)
        {
            foreach (var resourceUri in typeof(TController).GetResourceUris())
            {
                foreach (var method in typeof(TController).GetUnsupportedMethods(resourceUri))
                    Map(routes, resourceUri, Defaults(RestfulController.MethodNotSupportedAction, resourceUri), method);
            }
        }

        /// <summary>
        /// For every resource URI referenced in a ResourceActionAttribute,
        /// maps the HEAD method to a RestfulController action that knows
        /// how to respond appropriately.
        /// </summary>
        public virtual void MapHead(RouteCollection routes)
        {
            MapAllResources(routes, RestfulController.HeadAction, "HEAD");
        }

        /// <summary>
        /// For every resource URI referenced in a ResourceActionAttribute,
        /// maps the OPTIONS method to a RestfulController action that knows
        /// how to respond appropriately.
        /// </summary>
        /// <param name="routes"></param>
        public virtual void MapOptions(RouteCollection routes)
        {
            MapAllResources(routes, RestfulController.OptionsAction, "OPTIONS");
        }

        private static void MapAllResources(ICollection<RouteBase> routes, string actionName, string httpMethod)
        {
            foreach (var resourceUri in typeof(TController).GetResourceUris())
                Map(routes, resourceUri, Defaults(actionName, resourceUri), httpMethod);
        }

        private static void Map(ICollection<RouteBase> routes, string urlFormat,
            RouteValueDictionary defaults, string httpMethod)
        {
            routes.Add(new Route(urlFormat, defaults,
                new RouteValueDictionary {{"httpMethod", new HttpMethodConstraint(httpMethod)}},
                new MvcRouteHandler()));
        }

        private static RouteValueDictionary Defaults(string actionName)
        {
            return new RouteValueDictionary
            {
                {"controller", typeof(TController).GetControllerName()}, {"action", actionName}
            };
        }

        private static RouteValueDictionary Defaults(string actionName, string resourceUri)
        {
            var result = Defaults(actionName);
            result.Add("resourceUri", resourceUri);
            return result;
        }
    }
}