using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GitTools.IssueTrackers;
using Shouldly;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ArgumentTests
    {
        [Fact]
        public void VerifyProviderDescriptions()
        {
            var propertyInfo = typeof(GitReleaseNotesArguments).GetProperty("IssueTracker");
            var description = propertyInfo.GetCustomAttribute<DescriptionAttribute>();

            var issueTrackers = Enum.GetValues(typeof(IssueTracker)).Cast<IssueTracker>().Except(new[] { IssueTracker.Unknown });
            foreach (var issueTracker in issueTrackers)
            {
                description.Description.ShouldContain(issueTracker.ToString());
            }
        }
    }
}