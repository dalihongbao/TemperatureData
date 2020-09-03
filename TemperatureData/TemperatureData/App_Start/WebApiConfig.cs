using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Assignment2
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            // route for specific year and station
            // to generate monthly and daily data
            config.Routes.MapHttpRoute(
            name: "StationWebApi",
            routeTemplate: "api/{controller}/{station}/{year}",
            defaults: new
            {
                year = RouteParameter.Optional
            }
        );

          // route for all years or stations by year
          // to select year, station to view summary data for year
          config.Routes.MapHttpRoute(
              name: "WebApi",
              routeTemplate: "api/{controller}/"
          );
        }
    }
}
