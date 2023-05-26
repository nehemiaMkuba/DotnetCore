using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IPN.API.Models.Common
{
    /// <summary>
    /// Generic response status object
    /// </summary>
    public class ResponseStatus
    {
        /// <summary>
        /// Status code of the request
        /// </summary> 
        [DataMember(IsRequired = true)]
        public string Code { get; set; }

        /// <summary>
        /// Friendly message to be displayed to end-user after evaluating status code
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Message { get; set; }
    }

    /// <summary>
    /// It's a complex generic base object encapsulating a response object of a specified type alongside request status
    /// </summary>    
    [DataContract]
    public class ResponseObject<T>
    {
        /// <summary>
        /// Object containing status of a function call
        /// </summary>
        [DataMember(IsRequired = true)]
        public ResponseStatus Status { get; set; }

        /// <summary>
        /// Generic object containing a method's response
        /// </summary>
        [DataMember(IsRequired = true)]
        public IEnumerable<T> Data { get; set; } //dynamic[]

        /// <summary>
        /// Onject with Pagination Information
        /// </summary>
        [DataMember(IsRequired = true)]
        public Paging Paging { get; set; }

        public ResponseObject()
        {
            Status = new ResponseStatus { Code = "200", Message = "Request processed successfully" };
            Data = Enumerable.Empty<T>();
            Paging = new Paging { };
        }
    }

    /// <summary>
    /// Object containing paging parameters 
    /// </summary>
    [DataContract]
    public class Paging
    {
        /// <summary>
        /// Url of the subsequent page and its attributes
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Next { get; set; }

        /// <summary>
        /// Url of the previous page and its attributes
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Previous { get; set; }

        /// <summary>
        /// Total Count of all elements
        /// </summary>
        [DataMember(IsRequired = true)]
        public int Count { get; set; }
    }
}
