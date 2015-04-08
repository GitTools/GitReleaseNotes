using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Catel;
using GitReleaseNotes.Website.Models;
using GitReleaseNotes.Website.Models.Api;
using GitReleaseNotes.Website.Services;

namespace GitReleaseNotes.Website.Controllers.Api
{
    using System.Web.Http;

    [RoutePrefix("api/releasenotes")]
    public class ReleaseNotesController : ApiControllerBase
    {
        private readonly IReleaseNotesService _releaseNotesService;

        public ReleaseNotesController(IReleaseNotesService releaseNotesService)
        {
            Argument.IsNotNull(() => releaseNotesService);

            _releaseNotesService = releaseNotesService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Generate([FromBody] ReleaseNotesRequest releaseNotesRequest)
        {
            Argument.IsNotNull(() => releaseNotesRequest);

            var context = releaseNotesRequest.ToContext();
            context.AllTags = true;

            var releaseNotes = _releaseNotesService.GetReleaseNotes(context);
            
            return new HttpResponseMessage
            {
                Content = new JsonContent(releaseNotes)
            };
        }
    }
}
