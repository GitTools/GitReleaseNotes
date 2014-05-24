using System.IO;

namespace GitReleaseNotes.FileSystem
{
    class FileSystem : IFileSystem
    {
        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}