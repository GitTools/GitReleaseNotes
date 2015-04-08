namespace GitReleaseNotes.Website.Models.Api
{
    public class Response
    {
        public Response()
        {
            IsSuccess = true;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public static Response CreateError(string errorMessage)
        {
            return new Response
            {
                IsSuccess = false,
                Message = errorMessage
            };
        }

        public static Response CreateSuccess(string errorMessage = null)
        {
            return new Response
            {
                IsSuccess = true,
                Message = errorMessage
            };
        }
    }
}