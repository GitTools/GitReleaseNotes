namespace GitReleaseNotes.FileSystem
{
    public interface IFileSystem
    {
        void WriteAllText(string path, string contents);
        void WriteAllBytes(string path, byte[] contents);
        string ReadAllText(string path);
        bool FileExists(string path);
        string GetRepositoryWorkingDirectory(Context context);
    }
}