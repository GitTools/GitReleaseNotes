namespace GitReleaseNotes
{
    public class ReleaseNoteLine : IReleaseNoteLine
    {
        private readonly string line;

        public ReleaseNoteLine(string line)
        {
            this.line = line;
        }

        public string ToString(Categories categories)
        {
            return line;
        }
    }
}