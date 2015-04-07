namespace GitReleaseNotes
{
    // TODO: Move some parts to authentication context
    public class BitBucketContext
    {
        public string Repo { get; set; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecretKey { get; set; }
    }
}