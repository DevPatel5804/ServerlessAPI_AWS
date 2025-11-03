using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace serverless_auth.Utilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class XApiKeyUtilities : Attribute, IAsyncActionFilter
    {
        private const string APIKEY_HEADER_NAME = "X-API-KEY";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { Message = $"Missing {APIKEY_HEADER_NAME} header." });
                return;
            }

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = configuration.GetValue<string>("ApiKeys:XApiKey");

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API Key is not configured in appsettings.json or environment variables.");
                context.Result = new StatusCodeResult(500);
                return;
            }

            if (!apiKey.Equals(extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { Message = "Invalid API Key." });
                return;
            }

            await next();
        }
    }
}
