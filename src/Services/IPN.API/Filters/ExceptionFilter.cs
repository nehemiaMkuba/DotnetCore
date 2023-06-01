using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;

using Core.Domain.Exceptions;
using IPN.API.Models.Common;

namespace IPN.API.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<ExceptionFilter> logger;

        public ExceptionFilter(IWebHostEnvironment env, ILogger<ExceptionFilter> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message);
   
            if (context.Exception.GetType() == typeof(GenericException))
            {
                GenericException genericException = context.Exception as GenericException;

                ResponseObject<object> responseObject = new ResponseObject<object>
                {
                    Status = new ResponseStatus { Code = $"{(int)genericException.StatusCode}", Message = genericException.UserMessage }
                };

                context.Result = new HttpActionResult(responseObject, (int)genericException.StatusCode);
                context.HttpContext.Response.StatusCode = (int)genericException.StatusCode;
            }
            else
            {
                string genericMessage = "Sorry, your request could not be competed. If problem persists, please contact us for assistance";
                if (env.IsDevelopment())
                {
                    genericMessage = $"{context.Exception.Message} | {context.Exception.StackTrace}";
                }

                ResponseObject<object> responseObject = new ResponseObject<object>
                {
                    Status = new ResponseStatus { Code = $"{(int)HttpStatusCode.InternalServerError}", Message = genericMessage }
                };

                context.Result = new HttpActionResult(responseObject, (int)HttpStatusCode.InternalServerError);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.ExceptionHandled = true;
        }        
    }

}
