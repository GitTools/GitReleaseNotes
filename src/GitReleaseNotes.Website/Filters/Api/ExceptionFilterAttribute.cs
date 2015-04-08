namespace GitReleaseNotes.Website.Filters.Api
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;
    using Catel.Logging;
    using Models.Api;

    public class ExceptionFilterAttribute : System.Web.Http.Filters.ExceptionFilterAttribute
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            var baseUri = actionExecutedContext.Request.RequestUri.AbsolutePath;

            Log.Error(actionExecutedContext.Exception, "An error occurred while handling request '{0}'", baseUri);

            var message = "An unexpected error occurred, check server logs for more information";

            var knownException = actionExecutedContext.Exception as GitReleaseNotesException;
            if (knownException != null)
            {
                message = knownException.Message;
            }

            var response = Response.CreateError(message);
            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new JsonContent(response)
            };
        }
    }
}