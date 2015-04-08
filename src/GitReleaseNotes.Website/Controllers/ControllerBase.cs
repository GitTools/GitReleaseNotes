namespace GitReleaseNotes.Website.Controllers
{
    using System.Web.Mvc;

    public abstract class ControllerBase : Controller
    {
        public ControllerBase()
        {

        }

        public string UserIp
        {
            get { return Request.ServerVariables["REMOTE_ADDR"]; }
        }
    }
}