namespace IPN.API.Models.DTOs.Responses
{
    public class TokenDto
    {
        /// <summary>
        /// JWT token to be used in the subsequent calls for authorization
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Timestamp at which the token expires
        /// </summary>
        public long Expires { get; set; }

        /// <summary>
        /// Type of token
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }
}
