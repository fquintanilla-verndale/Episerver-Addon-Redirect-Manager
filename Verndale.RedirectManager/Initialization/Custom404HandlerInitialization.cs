using System;
using System.Web;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace Verndale.RedirectManager.Initialization
{
    /// <inheritdoc />
    /// <summary>
    /// Global File Not Found Handler, for handling Asp.net exceptions
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class Custom404HandlerInitialization : IInitializableHttpModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private static Injected<RequestHandler> RequestHandler { get; set; }
        private static Injected<ErrorHandler> ErrorHandler { get; set; }

        public void Initialize(InitializationEngine context)
        {
            Logger.Debug("Start Initialize()");
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }

        public void InitializeHttpEvents(HttpApplication application)
        {
            Logger.Debug("Start InitializeHttpEvents()");

            application.Error += OnError;
            application.EndRequest += OnEndRequest;
        }

        private void OnEndRequest(object sender, EventArgs eventArgs)
        {
            try
            {
                Logger.Debug("Start OnEndRequest()");
                RequestHandler.Service.Handle(GetContext());
            }
            catch (HttpException e)
            {
                Logger.Warning("Http error (headers already written or similar) on 404 handling.", e);
            }
            catch (Exception e)
            {
                Logger.Error("Error on 404 handling.", e);
                throw;
            }
        }

        private void OnError(object sender, EventArgs eventArgs)
        {
            try
            {
                ErrorHandler.Service.Handle(GetContext());
            }
            catch (HttpException e)
            {
                Logger.Warning("Http error (headers already written or similar) on 404 handling.", e);
            }
            catch (Exception e)
            {
                Logger.Error("Error on 404 handling.", e);
                throw;
            }
        }

        private static HttpContextBase GetContext()
        {
            var context = HttpContext.Current;
            if (context != null) return new HttpContextWrapper(context);

            Logger.Debug("No HTTPContext, returning");
            return null;
        }
    }
}