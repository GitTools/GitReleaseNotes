namespace GitReleaseNotes.Website.Filters.Api
{
    using System.Web.Http.Filters;
    using Models.Api;

    public class ApiResponseFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            var actionResponse = actionExecutedContext.Response;
            if (actionResponse == null)
            {
                return;
            }

            var content = actionResponse.Content;
            if (content == null)
            {
                var response = Response.CreateSuccess();
                actionExecutedContext.Response.Content = new JsonContent(response);
            }
        }
    }
}