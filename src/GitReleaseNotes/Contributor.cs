namespace GitReleaseNotes
{
    public class Contributor
    {
        public Contributor(string name, string username, string url)
        {
            Name = name;
            Username = username;
            Url = url;
        }

        public string Name { get; private set; }
        public string Username { get; private set; }
        public string Url { get; private set; }
    }
}