using System.ComponentModel.DataAnnotations;

namespace IPN.API.Models.DTOs.Requests
{
    public class EmailRequest
    {
        /// <summary>
        /// Identifier of user to update
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "userId must be provided")]
        [Range(1, long.MaxValue, ErrorMessage = "userId must be greater than 0")]
        public string UserId { get; set; }

        /// <summary>
        /// New email address
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "email must be provided")]
        [StringLength(150, ErrorMessage = "Maximum length for email is 150 characters")]
        public string Email { get; set; }
    }
}
