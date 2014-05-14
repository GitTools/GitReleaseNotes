namespace GitReleaseNotes.Git
{
    public interface ITaggedCommitFinder
    {
        TaggedCommit GetLastTaggedCommit();

        TaggedCommit FromFirstCommit();
    }
}