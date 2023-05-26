using System;
using System.ComponentModel.DataAnnotations;

namespace IPN.API.Models.DTOs.Requests
{
    /// <summary>
    /// Object to request access token
    /// </summary>
    public class TokenRequest
    {
        /// <summary>
        /// ApiKey as provided when registering
        /// </summary>        
        [Required(ErrorMessage = "apiKey must be provided", AllowEmptyStrings = false)]
        public Guid ApiKey { get; set; }

        /// <summary>
        /// AppSecret as provided
        /// </summary>
        [Required(ErrorMessage = "appSecret must be provided", AllowEmptyStrings = false)]
        [StringLength(128, ErrorMessage = "Maximum length for appSecret is 128 characters")]
        public string AppSecret { get; set; }
    }
}
