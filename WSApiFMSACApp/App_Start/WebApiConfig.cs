using System;
using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using log4net.Config;

namespace WSApiFMSACApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            #region Configuración Log4Net
            log4net.GlobalContext.Properties["LogDate"] = DateTime.Now.ToString("yyyyMMdd");
            XmlConfigurator.Configure();
            #endregion
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
        }
    }
}
