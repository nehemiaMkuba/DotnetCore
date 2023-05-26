using System;
using System.Net;

using Core.Domain.Infrastructure.Services;

namespace Core.Domain.Exceptions
{
    public class GenericException : Exception
    {
        public string ErrorCode { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string UserMessage { get; set; }    
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow.ToInstanceDate();    

        public GenericException() : base()
        {
        }

        public GenericException(string friendlyMessage, string errorCode, HttpStatusCode statusCode = HttpStatusCode.OK) : base(friendlyMessage)
        {
            UserMessage = friendlyMessage;
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public GenericException(string technicalMessage, string friendlyMessage, string errorCode, HttpStatusCode statusCode = HttpStatusCode.OK) : base(technicalMessage)
        {
            UserMessage = friendlyMessage;
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public GenericException(Exception exception, string friendlyMessage, string errorCode, HttpStatusCode statusCode = HttpStatusCode.OK) : base(friendlyMessage, exception)
        {
            UserMessage = friendlyMessage;
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}
