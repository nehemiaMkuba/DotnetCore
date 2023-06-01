using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IPN.API.Filters
{
    public class HttpActionResult : IActionResult
    {
        private readonly object _message;
        private readonly int _statusCode;

        public HttpActionResult(object message, int statusCode)
        {
            _message = message;
            _statusCode = statusCode;
        }

        async Task IActionResult.ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult = new ObjectResult(_message)
            {
                StatusCode = _statusCode
            };

            await objectResult.ExecuteResultAsync(context);
        }
    }

}
