using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitReleaseNotes.Git
{
    public interface IGitRepositoryContextFactory
    {
        GitRepositoryContext GetRepositoryContext();
    }
}
