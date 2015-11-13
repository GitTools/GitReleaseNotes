using System;
using System.IO;
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

            var tempDirectory = Path.Combine(Path.GetTempPath(), "GitTools", "GitReleaseNotes", Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempDirectory);

            try
            {
                var parameters = new ReleaseNotesGenerationParameters
                {
                    Repository = 
                    {
                        Url = releaseNotesRequest.RepositoryUrl,
                        Branch = releaseNotesRequest.RepositoryBranch
                    },
                    IssueTracker =
                    {
                        Server = releaseNotesRequest.IssueTrackerUrl,
                        ProjectId = releaseNotesRequest.IssueTrackerProjectId
                    },
                    WorkingDirectory = tempDirectory
                };

                parameters.AllTags = true;
                var releaseNotes = await releaseNotesService.GetReleaseNotesAsync(parameters);

                return new HttpResponseMessage
                {
                    Content = new JsonContent(releaseNotes)
                };
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }
}
