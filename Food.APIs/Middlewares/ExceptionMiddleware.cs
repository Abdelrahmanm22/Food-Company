using System.Net;
using System.Text.Json;
using Food.APIs.Errors;

namespace Food.APIs.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> logger;
        private readonly IHostEnvironment env;
        public ExceptionMiddleware(RequestDelegate Next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            next = Next;
            this.logger = logger;
            this.env = env;
        }
        // InvokeAsync
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                //Production ==>> Log ex in Database

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError; //enum System.Net.HttpStatusCode.InternalServerError ==>> 500


                var Response = env.IsDevelopment() ? new ApiServerErrorResponse(ex.Message, ex.StackTrace.ToString()) : new ApiServerErrorResponse();
                var Options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                context.Response.WriteAsync(JsonSerializer.Serialize(Response, Options));
            }
        }
    }
}
