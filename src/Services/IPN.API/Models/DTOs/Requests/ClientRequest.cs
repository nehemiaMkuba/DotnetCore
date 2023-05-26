using System.ComponentModel.DataAnnotations;

namespace IPN.API.Models.DTOs.Requests
{
    /// <summary>
    /// Object with parameters required to register a resource client
    /// </summary>
    public class ClientRequest
    {
        /// <summary>
        /// Name of client requesting resource access
        /// </summary>
        [Required(ErrorMessage = "Name must be provided", AllowEmptyStrings = false)]
        [StringLength(50, ErrorMessage = "Maximum length for name is 50 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Contact email must be provided
        /// </summary>
        [Required(ErrorMessage = "contactEmail must be provided", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150, ErrorMessage = "Maximum length for contactEmail is 150 characters")]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Short description of the intented API use
        /// </summary>
        [Required(ErrorMessage = "description must be provided", AllowEmptyStrings = false)]
        [StringLength(250, ErrorMessage = "Maximum length for description is 250 characters")]
        public string Description { get; set; }
    }
}
