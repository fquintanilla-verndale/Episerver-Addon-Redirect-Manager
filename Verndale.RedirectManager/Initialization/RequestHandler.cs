using System;
using System.IO;
using System.Linq;
using System.Web;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Verndale.RedirectManager.Repositories;
using Verndale.RedirectManager.Util;

namespace Verndale.RedirectManager.Initialization
{
    public class RequestHandler
    {
        private const string DefIgnoredExtensions = "jpg,gif,png,css,js,ico,swf,woff";

        private static readonly ILogger Logger = LogManager.GetLogger();

        public virtual void Handle(HttpContextBase context)
        {
            if (context == null) return;

            if (context.Response.StatusCode != 404)
            {
                LogDebug("Not a 404 response.", context);
                return;
            }

            LogDebug("Handling 404 request.", context);

            var notFoundUri = context.Request.Url;

            if (IsResourceFile(notFoundUri))
            {
                LogDebug("Skipping resource file.", context);
                return;
            }

            var query = context.Request.ServerVariables["QUERY_STRING"];

            // avoid duplicate log entries
            if (query != null && query.StartsWith("404;"))
            {
                LogDebug("Skipping request with 404; in the query string.", context);
                return;
            }

            var redirectManagerRepository = new RedirectManagerRepository();
            var redirect = redirectManagerRepository.Get(notFoundUri?.AbsoluteUri);

            if (redirect != null)
            {
                LogDebug("Handled URL", context);
                var newUrl = redirect.NewUrl.Trim();

                if (!newUrl.Contains("http"))
                {
                    newUrl = Path.Combine(GetDefaultWebSite(), newUrl);
                }
                
                if (redirect.IncludeQuery && context.Request.Url != null)
                {
                    newUrl = newUrl + context.Request.Url.Query;
                }

                Logger.Debug("New absolute Url: " + newUrl);
                
                if (redirect.Type == 301)
                {
                    context
                        .ClearServerError()
                        .RedirectPermanent(newUrl);
                }
                else
                {
                    context
                        .ClearServerError()
                        .Redirect(newUrl);
                }
            }
            else
            {
                LogDebug("Not handled. Current URL is ignored or no redirect found.", context);

                context
                    .ClearServerError()
                    .SetStatusCode(404);
            }
        }

        public virtual bool IsResourceFile(Uri notFoundUri)
        {
            var extension = notFoundUri.AbsolutePath;
            var extPos = extension.LastIndexOf('.');

            if (extPos <= 0) return false;

            extension = extension.Substring(extPos + 1);
            var ignoredExtensions = DefIgnoredExtensions.Split(',');

            if (ignoredExtensions.Contains(extension))
            {
                // Ignoring 404 rewrite of known resource extension
                Logger.Debug("Ignoring rewrite of '{0}'. '{1}' is a known resource extension", notFoundUri.ToString(), extension);
                return true;
            }
            return false;
        }

        #region Private Methods

        private string GetDefaultWebSite()
        {
            var siteDefinitionRepository = ServiceLocator.Current.GetInstance<SiteDefinitionRepository>();
            var siteDefinitions = siteDefinitionRepository.List().FirstOrDefault();

            var url = siteDefinitions != null ? siteDefinitions.SiteUrl.AbsoluteUri : string.Empty;

            Logger.Debug("Default website url: " + url);

            return url;
        }

        #endregion

        private void LogDebug(string message, HttpContextBase context)
        {
            Logger.Debug(
                $"{{0}}{Environment.NewLine}Request URL: {{1}}{Environment.NewLine}Response status code: {{2}}",
                message, context?.Request.Url, context?.Response.StatusCode);
        }
    }
}