using System.Reflection;
using Atlassian.Jira.Remote;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    //public static class JiraExtensions
    //{
    //    #region Methods

    //    public static string Authenticate(this JiraSoapServiceClient client, Context context)
    //    {
    //        Argument.IsNotNull(() => client);
    //        Argument.IsNotNull(() => context);

    //        return client.login(context.UserName, context.Password);
    //    }

    //    public static string GetToken(this Atlassian.Jira.Jira jira)
    //    {
    //        Argument.IsNotNull("jira", jira);

    //        var field = jira.GetType().GetField("_token", BindingFlags.Instance | BindingFlags.NonPublic);
    //        return (string) field.GetValue(jira);
    //    }

    //    public static IJiraRemoteService GetJiraService(this Atlassian.Jira.Jira jira)
    //    {
    //        Argument.IsNotNull("jira", jira);

    //        return PropertyHelper.GetPropertyValue<IJiraRemoteService>(jira, "RemoteService");
    //    }

    //    #endregion
    //}
}