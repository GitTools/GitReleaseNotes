namespace GitReleaseNotes
{
    public interface IFileSystem
    {
        void WriteAllText(string path, string contents);
        string ReadAllText(string path);
    }
}