namespace GitReleaseNotes
{
    public class BlankLine : IReleaseNoteLine
    {
        public string ToString(string[] categories)
        {
            return string.Empty;
        }
    }
}