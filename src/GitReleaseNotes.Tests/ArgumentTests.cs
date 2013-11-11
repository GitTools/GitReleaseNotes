using System;
using System.ComponentModel;
using System.Reflection;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ArgumentTests
    {
        [Fact]
        public void VerifyProviderDescriptions()
        {
            var propertyInfo = typeof (GitReleaseNotesArguments).GetProperty("IssueTracker");
            var description = propertyInfo.GetCustomAttribute<DescriptionAttribute>();

            foreach (IssueTracker issueTracker in Enum.GetValues(typeof(IssueTracker)))
            {
                Assert.True(description.Description.Contains(issueTracker.ToString()));
            }
        }
    }
}