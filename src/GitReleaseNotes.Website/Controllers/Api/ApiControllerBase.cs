namespace GitReleaseNotes.Website.Controllers.Api
{
    using System.Web.Http;
    using Filters.Api;

    [ApiResponseFilter]
    [ExceptionFilterAttribute]
    public abstract class ApiControllerBase : ApiController
    {
        public ApiControllerBase()
        {
            
        }


    }
}