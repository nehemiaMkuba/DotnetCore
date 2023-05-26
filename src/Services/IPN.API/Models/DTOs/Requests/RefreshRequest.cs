using System.ComponentModel.DataAnnotations;

namespace IPN.API.Models.DTOs.Requests
{
    public class RefreshRequest
    {
        /// <summary>
        /// AppSecret as provided
        /// </summary>
        [Required(ErrorMessage = "Secret must be provided", AllowEmptyStrings = false)]
        public string AppSecret { get; set; }
    }
}
