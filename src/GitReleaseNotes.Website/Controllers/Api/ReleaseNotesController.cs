using System.Net.Http;
using System.Threading.Tasks;
using Catel;
using GitReleaseNotes.Website.Models.Api;
using GitReleaseNotes.Website.Services;

namespace GitReleaseNotes.Website.Controllers.Api
{
    using System.Web.Http;

    [RoutePrefix("api/releasenotes")]
    public class ReleaseNotesController : ApiControllerBase
    {
        private readonly IReleaseNotesService releaseNotesService;

        public ReleaseNotesController(IReleaseNotesService releaseNotesService)
        {
            Argument.IsNotNull(() => releaseNotesService);

            this.releaseNotesService = releaseNotesService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Generate([FromBody] ReleaseNotesRequest releaseNotesRequest)
        {
            Argument.IsNotNull(() => releaseNotesRequest);

            var context1 = new ReleaseNotesGenerationParameters
            {
                RepositorySettings =
                {
                    Url = releaseNotesRequest.RepositoryUrl,
                    Branch = releaseNotesRequest.RepositoryBranch
                },
                IssueTracker = {ProjectId = releaseNotesRequest.IssueTrackerProjectId}
            };

            var context = context1;
            context.AllTags = true;
            var releaseNotes = await releaseNotesService.GetReleaseNotesAsync(context);
            
            return new HttpResponseMessage
            {
                Content = new JsonContent(releaseNotes)
            };
        }
    }
}
