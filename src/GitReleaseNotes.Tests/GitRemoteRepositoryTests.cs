using System;
using System.IO;
using System.Linq;
using GitReleaseNotes.Git;
using LibGit2Sharp;
using Shouldly;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class GitRemoteRepositoryTests
    {
        [Fact]
        public void CanGetRemoteRepoContext()
        {
            // Arrange
            // 1. Create a local repo to serve as the source / origin for our clone.
            var originRepoDir = TestGitRepoUtils.GetUniqueTempFolder("testOriginGitRepo");
            var testOriginRepo = TestGitRepoUtils.CreateEmptyTestRepo(originRepoDir);
            const string expectedDefaultBranchName = "master";

            // Construct the arguments necessary for cloning the origin repo.
            var remoteArgs = new GitRemoteRepositoryContextFactory.RemoteRepoArgs();
            var creds = new DefaultCredentials();
            remoteArgs.Credentials = creds;
            var desinationDirForClone = TestGitRepoUtils.GetUniqueTempFolder("testClonedGitRepo"); // Path.Combine(currentDir, "testClonedGitRepo", Guid.NewGuid().ToString("N"));
            remoteArgs.DestinationPath = desinationDirForClone;          
            remoteArgs.Url = testOriginRepo.Info.Path; // This could be the Url of the git repo, but as this is a unit test, we are using a local file path.
          
            var remoteRepoContextFactory = new GitRemoteRepositoryContextFactory(remoteArgs);

            // Act
            using (var repoContext = remoteRepoContextFactory.GetRepositoryContext())
            {
                // Assert
                repoContext.IsRemote.ShouldBe(true);
                Directory.Exists(Path.Combine(desinationDirForClone, ".git")).ShouldBe(true);

                var currentBranch = repoContext.Repository.Head.CanonicalName;
                currentBranch.ShouldEndWith(expectedDefaultBranchName); // cloned repo should default to master branch.
            }
        }

        [Theory]
        [InlineData("master")]
        [InlineData("feature/somefeature")]
        public void CanGetRemoteRepoContextWithHeadAtBranchName(string branchName)
        {
            // Arrange
            // Create a local repo to serve as the origin for our clone.

            var originRepoDir = TestGitRepoUtils.GetUniqueTempFolder("testOriginGitRepo");
            
            //Path.Combine(currentDir, "", Guid.NewGuid().ToString("N"));

            using(var testOriginRepo = TestGitRepoUtils.CreateRepoWithBranch(originRepoDir, branchName))
            {
                // Construct the arguments necessary for cloning this origin repo.
                var remoteArgs = new GitRemoteRepositoryContextFactory.RemoteRepoArgs();
                var creds = new DefaultCredentials();
                remoteArgs.Credentials = creds;

                var desinationDirForClone = TestGitRepoUtils.GetUniqueTempFolder("testClonedGitRepo"); // Path.Combine(currentDir, "testClonedGitRepo", Guid.NewGuid().ToString("N"));
                remoteArgs.DestinationPath = desinationDirForClone;
               
                remoteArgs.Url = testOriginRepo.Info.Path; // This could be the Url of the git repo, but as this is a unit test, we are using a local file path.

                // This is the sut.
                var remoteRepoContextFactory = new GitRemoteRepositoryContextFactory(remoteArgs);

                using (var repoContext = remoteRepoContextFactory.GetRepositoryContext())
                {
                    // Act
                    repoContext.PrepareRemoteRepoForUse(branchName);

                    // The cloned repo should now be set to the specified branch name.
                    var currentBranch = repoContext.Repository.Head.CanonicalName;
                    currentBranch.ShouldEndWith(branchName);
                }
            }
        }

        [Theory]
        [InlineData("master", true)]
        [InlineData("feature/somefeature", false)]
        public void CanGetRemoteRepoContextAndCheckoutReleaseNotesIfExists(string branchName, bool shouldHaveReleaseNotesFile)
        {
            // Arrange
            // Create a local repo to serve as the origin for our clone, - with a release notes file.
            var currentDir = Environment.CurrentDirectory;
            var originRepoDir = TestGitRepoUtils.GetUniqueTempFolder("testOriginGitRepo");  

            using (var testOriginRepo = TestGitRepoUtils.CreateRepoWithBranch(originRepoDir, branchName))
            {
                string releaseNotesFileName = Guid.NewGuid().ToString();
                // If a release notes file should be added, switch to the branch and add one.
                if (shouldHaveReleaseNotesFile)
                {
                    // switch head to the branch.
                    var branchInfo = new GitBranchNameInfo(branchName);
                    var targetBranchName = branchInfo.GetCanonicalBranchName();
                    var newHead = testOriginRepo.Refs.FirstOrDefault(localRef => string.Equals(localRef.CanonicalName, targetBranchName));
                    testOriginRepo.Refs.UpdateTarget(testOriginRepo.Refs.Head, newHead);

                    // commit a releasenotes file to the branch.
                    var releaseNotesFilePath = Path.Combine(testOriginRepo.Info.WorkingDirectory, releaseNotesFileName);
                    File.WriteAllText(releaseNotesFilePath, @"Some customised release notes contents...");
                    TestGitRepoUtils.CommitFile(testOriginRepo, releaseNotesFilePath, "Added test release notes file to repo");
                }

                // Construct the arguments necessary for cloning the origin repo.
                var remoteArgs = new GitRemoteRepositoryContextFactory.RemoteRepoArgs();
                var creds = new DefaultCredentials();
                remoteArgs.Credentials = creds;
                var desinationDirForClone = TestGitRepoUtils.GetUniqueTempFolder("testClonedGitRepo"); // Path.Combine(currentDir, "testClonedGitRepo", Guid.NewGuid().ToString("N"));
                remoteArgs.DestinationPath = desinationDirForClone;
                remoteArgs.Url = testOriginRepo.Info.Path; // This could be the Url of the git repo, but as this is a unit test, we are using a local file path.
              
                var remoteRepoContextFactory = new GitRemoteRepositoryContextFactory(remoteArgs);
                using (var repoContext = remoteRepoContextFactory.GetRepositoryContext())
                {                    
                    repoContext.PrepareRemoteRepoForUse(branchName);

                    // Act.
                    repoContext.CheckoutFilesIfExist(releaseNotesFileName);

                    // Assert.
                    var releaseNotesFilePath = Path.Combine(desinationDirForClone,releaseNotesFileName);
                    File.Exists(releaseNotesFilePath).ShouldBe(shouldHaveReleaseNotesFile);
                }
            }
        }
    }
}
