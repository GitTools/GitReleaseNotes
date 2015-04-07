using System.IO;

namespace GitReleaseNotes.Git
{
    public class GitDirFinder
    {
        public static string TreeWalkForGitDir(string workingDirectory)
        {
            while (true)
            {
                var gitDir = Path.Combine(workingDirectory, @".git");
                if (Directory.Exists(gitDir))
                {
                    return gitDir;
                }

                var parent = Directory.GetParent(workingDirectory);
                if (parent == null)
                {
                    break;
                }

                workingDirectory = parent.FullName;
            }

            return null;
        }
    }
}