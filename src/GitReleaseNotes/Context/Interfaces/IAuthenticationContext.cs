namespace GitReleaseNotes
{
    public interface IAuthenticationContext
    {
        string Username { get; set; }
        string Password { get; set; }
        string Token { get; set; }
    }
}