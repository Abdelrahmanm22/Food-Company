namespace Food.APIs.Errors
{
    public class ApiServerErrorResponse : ApiErrorResponse
    {
        public string? Details { get; set; }
        public ApiServerErrorResponse(string? message = null,string? details = null):base(500,message)
        {
            Details = details;
        }
    }
}
