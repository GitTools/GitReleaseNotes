using System;
using System.IO;

namespace GitReleaseNotes.FileSystem
{
    public class FileSystem : IFileSystem
    {
        public virtual string GetRepositoryWorkingDirectory(Context context)
        {
            if (!string.IsNullOrWhiteSpace(context.Repository.Directory))
            {
                return context.Repository.Directory;
            }

            var key = context.GetContextKey();

            var tempDirectory = Path.Combine(Path.GetTempPath(), "GitReleaseNotes", key);
            return tempDirectory;
        }

        public virtual void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public virtual void WriteAllBytes(string path, byte[] contents)
        {
            File.WriteAllBytes(path, contents);
        }

        public virtual string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}