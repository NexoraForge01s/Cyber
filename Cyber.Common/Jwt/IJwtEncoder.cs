using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

using static JWT.Internal.EncodingHelper;

namespace JWT
{
    /// <summary>
    /// Represents a JWT encoder.
    /// </summary>
    public interface IJwtEncoder
    {
        /// <summary>
        /// Creates a JWT given a header, a payload, the signing key, and the algorithm to use.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload. Use a JSON-serializable type (AOT-safe).</typeparam>
        /// <param name="extraHeaders">An arbitrary set of extra headers. Will be augmented with the standard "typ" and "alg" headers</param>
        /// <param name="payload">The payload to be serialized into the JWT body</param>
        /// <param name="payloadTypeInfo">The JsonTypeInfo for AOT-safe serialization of payload</param>
        /// <param name="key">The key bytes used to sign the token</param>
        /// <returns>The generated JWT</returns>
        string Encode<TPayload>(IDictionary<string, object> extraHeaders, TPayload payload, JsonTypeInfo<TPayload> payloadTypeInfo, byte[] key);
    }

    /// <summary>
    /// Extension methods for <seealso cref="IJwtEncoder" />
    ///</summary>
    public static class JwtEncoderExtensions
    {
        /// <summary>
        /// Creates a JWT given a payload, the signing key, and the algorithm to use.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload. Use a JSON-serializable type (AOT-safe).</typeparam>
        /// <param name="encoder">The encoder instance</param>
        /// <param name="payload">The payload to be serialized into the JWT body</param>
        /// <param name="payloadTypeInfo">The JsonTypeInfo for AOT-safe serialization of payload</param>
        /// <param name="key">The key used to sign the token</param>
        /// <returns>The generated JWT</returns>
        /// <exception cref="ArgumentNullException" />
        public static string Encode<TPayload>(this IJwtEncoder encoder, TPayload payload, JsonTypeInfo<TPayload> payloadTypeInfo, string key) =>
            encoder.Encode<TPayload>(null, payload, payloadTypeInfo, key is not null ? GetBytes(key) : null);

        /// <summary>
        /// Creates a JWT given a payload, the signing key, and the algorithm to use.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload. Use a JSON-serializable type (AOT-safe).</typeparam>
        /// <param name="encoder">The encoder instance</param>
        /// <param name="payload">The payload to be serialized into the JWT body</param>
        /// <param name="payloadTypeInfo">The JsonTypeInfo for AOT-safe serialization of payload</param>
        /// <param name="key">The key used to sign the token</param>
        /// <returns>The generated JWT</returns>
        /// <exception cref="ArgumentNullException" />
        public static string Encode<TPayload>(this IJwtEncoder encoder, TPayload payload, JsonTypeInfo<TPayload> payloadTypeInfo, byte[] key) =>
            encoder.Encode<TPayload>(null, payload, payloadTypeInfo, key);

        /// <summary>
        /// Creates a JWT given a set of arbitrary extra headers, a payload, the signing key, and the algorithm to use.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload. Use a JSON-serializable type (AOT-safe).</typeparam>
        /// <param name="encoder">The encoder instance</param>
        /// <param name="extraHeaders">An arbitrary set of extra headers. Will be augmented with the standard "typ" and "alg" headers</param>
        /// <param name="payload">The payload to be serialized into the JWT body</param>
        /// <param name="payloadTypeInfo">The JsonTypeInfo for AOT-safe serialization of payload</param>
        /// <param name="key">The key bytes used to sign the token</param>
        /// <returns>The generated JWT</returns>
        /// <exception cref="ArgumentNullException" />
        public static string Encode<TPayload>(this IJwtEncoder encoder, IDictionary<string, object> extraHeaders, TPayload payload, JsonTypeInfo<TPayload> payloadTypeInfo, string key) =>
            encoder.Encode<TPayload>(extraHeaders, payload, payloadTypeInfo, GetBytes(key));
    }
}
