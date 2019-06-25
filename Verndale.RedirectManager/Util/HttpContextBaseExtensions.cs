using System.Web;
using EPiServer.Logging;

namespace Verndale.RedirectManager.Util
{
    public static class HttpContextBaseExtensions
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        public static HttpContextBase ClearServerError(this HttpContextBase context)
        {
            Logger.Debug("ClearServerError");

            context.Server.ClearError();
            return context;
        }

        public static HttpContextBase SetStatusCode(this HttpContextBase context, int statusCode)
        {
            Logger.Debug("SetStatusCode");

            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.StatusCode = statusCode;
            return context;
        }

        public static HttpContextBase RedirectPermanent(this HttpContextBase context, string url)
        {
            Logger.Debug("RedirectPermanent");

            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.RedirectPermanent(url, endResponse: false);
            return context;
        }

        public static HttpContextBase Redirect(this HttpContextBase context, string url)
        {
            Logger.Debug("Redirect");

            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.Redirect(url, endResponse: false);
            return context;
        }
    }
}