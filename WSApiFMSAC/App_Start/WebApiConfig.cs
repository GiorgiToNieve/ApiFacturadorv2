using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Net.Http.Headers;
using System.Web.Http;

namespace WSApiFMSAC
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "servicio/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            ); //If you like XML to be return to client, remove below line

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.MessageHandlers.Insert(0,
                new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));

            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));


        }
    }
}
