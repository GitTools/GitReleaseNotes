namespace GitReleaseNotes.Website
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Owin;

    public partial class Startup
    {
        private void ConfigureJson(IAppBuilder app)
        {
            // http://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}