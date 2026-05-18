namespace Food.APIs.Errors
{
    public class ApiErrorResponse
    {
        public int status { get; set; }
        public string? message { get; set; }

        public ApiErrorResponse(int status,string? message = null)
        {
            this.status = status;
            this.message = message ?? GetDefaultMessageForStatusCode(status);
        }
        private string? GetDefaultMessageForStatusCode(int? status)
        {
            return status switch
            {
                400 => "Bad Request",
                401 => "You are not Authorized",
                404 => "Resource Not Found",
                500 => "Internal Server Error",
                _ => null,
            };
        }
    }
}
