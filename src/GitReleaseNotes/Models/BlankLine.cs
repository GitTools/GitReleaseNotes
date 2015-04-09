namespace GitReleaseNotes
{
    public class BlankLine : IReleaseNoteLine
    {
        public string ToString(Categories categories)
        {
            return string.Empty;
        }
    }
}