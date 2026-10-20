using System.Text.Json.Serialization;

namespace JWT.Builder
{
    /// <summary>
    /// JSON header model with predefined parameter names specified by RFC 7515, see https://tools.ietf.org/html/rfc7515
    /// </summary>
    public class JwtHeader
    {
        [JsonPropertyName("typ")]
        public string Type { get; set; }

        [JsonPropertyName("cty")]
        public string ContentType { get; set; }

        [JsonPropertyName("alg")]
        public string Algorithm { get; set; }

        [JsonPropertyName("kid")]
        public string KeyId { get; set; }

        [JsonPropertyName("x5u")]
        public string X5u { get; set; }

        [JsonPropertyName("x5c")]
        public string[] X5c { get; set; }

        [JsonPropertyName("x5t")]
        public string X5t { get; set; }
    }
}
