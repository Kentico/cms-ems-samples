using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;

using DancingGoat.Infrastructure;
using DancingGoat.Helpers;

using Kentico.Web.Mvc;

namespace DancingGoat
{
    /// <summary>
    /// Provides route configuration for application.
    /// </summary>
    public class RouteConfig
    {
        private static RouteCollection RegisterNewsRoutes(RouteCollection routes)
        {

            foreach (var culture in CultureUrlHelper.NewsUrlSlugCultureVersions)
            {
                // Map the index page routes.
                var route = routes.MapRoute(
                    name: $"NewsIndex-{culture.Key}",
                    url: "{culture}/" + culture.Value,
                    defaults: new { controller = "News", action = "Index" },
                    constraints: new { culture = new SiteCultureConstraint() }
                );

                route.RouteHandler = new MultiCultureMvcRouteHandler(CultureUrlHelper.DefaultCulture);

                // Map the detail page routes.
                route = routes.MapRoute(
                    name: $"NewsDetail-{culture.Key}",
                    url: "{culture}/" + culture.Value + "/{urlSlug}",
                    defaults: new { controller = "News", action = "Detail" },
                    constraints: new { culture = new SiteCultureConstraint(), urlSlug = new RegexRouteConstraint(@"[\w\d-_]") }
                );

                route.RouteHandler = new MultiCultureMvcRouteHandler(CultureUrlHelper.DefaultCulture);
            }

            return routes;
        }

        /// <summary>
        /// Register custom routes to given <paramref name="routes"/> collection.
        /// </summary>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Map routes to Kentico HTTP handlers and features enabled in ApplicationConfig.cs
            // Always map the Kentico routes before adding other routes. Issues may occur if Kentico URLs are matched by a general route, for example images might not be displayed on pages
            routes.Kentico().MapRoutes();

            // Redirect to administration site if the path is "admin"
            routes.MapRoute(
                name: "Admin",
                url: "admin",
                defaults: new { controller = "AdminRedirect", action = "Index" }
            );

            routes.MapRoute(
                name: "ImageUploader",
                url: "Api/Image/{action}/{pageId}",
                defaults: new { controller = "ImageUploader", action = "Upload" },
                constraints: new { pageId = new IntRouteConstraint() }
            );

            routes = RegisterNewsRoutes(routes);

            var route = routes.MapRoute(
                name: "Article",
                url: "{culture}/Articles/{guid}/{pageAlias}",
                defaults: new { culture = CultureUrlHelper.DefaultCulture.Name, controller = "Articles", action = "Show" },
                constraints: new { culture = new SiteCultureConstraint(), guid = new GuidRouteConstraint() }
            );

            // A route value determines the culture of the current thread
            route.RouteHandler = new MultiCultureMvcRouteHandler(CultureUrlHelper.DefaultCulture);

            route = routes.MapRoute(
                name: "Store",
                url: "{culture}/Store/{controller}",
                defaults: new { culture = CultureUrlHelper.DefaultCulture.Name, action = "Index" },
                constraints: new { culture = new SiteCultureConstraint(), controller = "Coffees|Brewers" }
            );

            // A route value determines the culture of the current thread
            route.RouteHandler = new MultiCultureMvcRouteHandler(CultureUrlHelper.DefaultCulture);

            route = routes.MapRoute(
                name: "LandingPage",
                url: "{culture}/LandingPage/{pageAlias}",
                defaults: new { culture = CultureUrlHelper.DefaultCulture.Name, controller = "LandingPage", action = "Index" },
                constraints: new { culture = new SiteCultureConstraint() }
            );

            // A route value determines the culture of the current thread
            route.RouteHandler = new MultiCultureMvcRouteHandler(CultureUrlHelper.DefaultCulture);

            route = routes.MapRoute(
                name: "Product",
                url: "{culture}/Product/{guid}/{productAlias}",
                defaults: new { culture = CultureUrlHelper.DefaultCulture.Name, controller = "Product", action = "Detail" },
                constraints: new { culture = new SiteCultureConstraint(), guid = new GuidRouteConstraint() }
            );

            // A route value determines the culture of the current thread
            route.RouteHandler = new MultiCultureMvcRouteHandler(CultureUrlHelper.DefaultCulture);

            route = routes.MapRoute(
                name: "Default",
                url: "{culture}/{controller}/{action}",
                defaults: new { culture = CultureUrlHelper.DefaultCulture.Name, controller = "Home", action = "Index" },
                constraints: new { culture = new SiteCultureConstraint() }
            );

            // A route value determines the culture of the current thread
            route.RouteHandler = new MultiCultureMvcRouteHandler(CultureUrlHelper.DefaultCulture);

            // Display a custom view for HTTP errors
            routes.MapRoute(
                name: "HttpErrors",
                url: "HttpErrors/{action}",
                defaults: new { controller = "HttpErrors" }
            );
        }
    }
}
