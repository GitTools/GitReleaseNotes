namespace GitReleaseNotes
{
    public abstract class AuthenticationContext : IAuthenticationContext
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }
    }
}