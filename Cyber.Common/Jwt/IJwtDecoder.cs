using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using JWT.Exceptions;

using static JWT.Internal.EncodingHelper;

namespace JWT
{
    /// <summary>
    /// Represents a JWT decoder.
    /// </summary>
    public interface IJwtDecoder
    {
        #region DecodeHeader

        /// <summary>
        /// Given a JWT, decodes it and return the header.
        /// </summary>
        /// <param name="token">The JWT</param>
        string DecodeHeader(string token);

        /// <summary>
        /// Given a JWT, decodes it and return the header as a typed object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the header into.</typeparam>
        /// <param name="jwt">The JWT</param>
        /// <param name="headerTypeInfo">The JsonTypeInfo for AOT-safe deserialization of header</param>
        T DecodeHeader<T>(JwtParts jwt, JsonTypeInfo<T> headerTypeInfo);

        #endregion

        #region Decode
        /// <summary>
        /// Given a JWT, decodes it and return the payload.
        /// </summary>
        /// <param name="jwt">The JWT</param>
        /// <param name="verify">Whether to verify the signature (default is true)</param>
        /// <returns>A string containing the JSON payload</returns>
        string Decode(JwtParts jwt, bool verify);

        /// <summary>
        /// Given a JWT, decodes it and return the payload.
        /// </summary>
        /// <param name="jwt">The JWT</param>
        /// <param name="key">The key that were used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is true)</param>
        /// <returns>A string containing the JSON payload</returns>
        string Decode(JwtParts jwt, byte[] key, bool verify);

        /// <summary>
        /// Given a JWT, decodes it and return the payload.
        /// </summary>
        /// <param name="jwt">The JWT</param>
        /// <param name="keys">The keys provided which one of them was used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is true)</param>
        /// <returns>A string containing the JSON payload</returns>
        string Decode(JwtParts jwt, byte[][] keys, bool verify);

        #endregion

        #region DecodeToObject<T>

        /// <summary>
        /// Given a JWT, decodes it and return the payload as a typed object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the payload into.</typeparam>
        /// <param name="jwt">The JWT</param>
        /// <param name="payloadTypeInfo">The JsonTypeInfo for AOT-safe deserialization of payload</param>
        /// <param name="verify">Whether to verify the signature (default is true)</param>
        /// <returns>A deserialized object representing the payload</returns>
        T DecodeToObject<T>(JwtParts jwt, JsonTypeInfo<T> payloadTypeInfo, bool verify);

        /// <summary>
        /// Given a JWT, decodes it and return the payload as a typed object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the payload into.</typeparam>
        /// <param name="jwt">The JWT</param>
        /// <param name="payloadTypeInfo">The JsonTypeInfo for AOT-safe deserialization of payload</param>
        /// <param name="key">The key that was used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is true)</param>
        /// <returns>A deserialized object representing the payload</returns>
        T DecodeToObject<T>(JwtParts jwt, JsonTypeInfo<T> payloadTypeInfo, byte[] key, bool verify);

        /// <summary>
        /// Given a JWT, decodes it and return the payload as a typed object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the payload into.</typeparam>
        /// <param name="jwt">The JWT</param>
        /// <param name="payloadTypeInfo">The JsonTypeInfo for AOT-safe deserialization of payload</param>
        /// <param name="keys">The keys which one of them was used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is true)</param>
        /// <returns>A deserialized object representing the payload</returns>
        T DecodeToObject<T>(JwtParts jwt, JsonTypeInfo<T> payloadTypeInfo, byte[][] keys, bool verify);

        #endregion
    }

    /// <summary>
    /// Extension methods for <seealso cref="IJwtDecoder" />>
    ///</summary>
    public static class JwtDecoderExtensions
    {
        #region Decode

        /// <summary>
        /// Given a JWT, decodes it and return the payload.
        /// </summary>
        /// <param name="decoder">The decoder instance</param>
        /// <param name="token">The JWT</param>
        /// <param name="verify">Whether to verify the signature (default is <c>true</c>)</param>
        /// <returns>A string containing the JSON payload</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidTokenPartsException" />
        public static string Decode(this IJwtDecoder decoder, string token, bool verify = true) =>
            decoder.Decode(new JwtParts(token), (byte[])null, verify);

        /// <summary>
        /// Given a JWT, decodes it, and return the payload.
        /// </summary>
        /// <param name="decoder">The decoder instance</param>
        /// <param name="token">The JWT</param>
        /// <param name="key">The key that was used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is <c>true</c>)</param>
        /// <returns>A string containing the JSON payload</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidTokenPartsException" />
        public static string Decode(this IJwtDecoder decoder, string token, byte[] key, bool verify = true) =>
            decoder.Decode(new JwtParts(token), new[] { key }, verify);

        /// <summary>
        /// Given a JWT, decodes it, and return the payload.
        /// </summary>
        /// <param name="decoder">The decoder instance</param>
        /// <param name="token">The JWT</param>
        /// <param name="keys">The keys that were used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is <c>true</c>)</param>
        /// <returns>A string containing the JSON payload</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidTokenPartsException" />
        public static string Decode(this IJwtDecoder decoder, string token, byte[][] keys, bool verify = true) =>
            decoder.Decode(new JwtParts(token), keys, verify);

        /// <summary>
        /// Given a JWT, decodes it, and return the payload as an dictionary.
        /// </summary>
        /// <param name="decoder">The decoder instance</param>
        /// <param name="token">The JWT</param>
        /// <param name="key">The key that was used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is <c>true</c>)</param>
        /// <returns>An object representing the payload</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public static string Decode(this IJwtDecoder decoder, string token, string key, bool verify = true) =>
            decoder.Decode(token, new[] { key }, verify);

        /// <summary>
        /// Given a JWT, decodes it, and return the payload as an dictionary.
        /// </summary>
        /// <param name="decoder">The decoder instance</param>
        /// <param name="token">The JWT</param>
        /// <param name="keys">The key which one of them was used to sign the JWT</param>
        /// <param name="verify">Whether to verify the signature (default is <c>true</c>)</param>
        /// <returns>An object representing the payload</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public static string Decode(this IJwtDecoder decoder, string token, string[] keys, bool verify = true) =>
            decoder.Decode(token, GetBytes(keys), verify);

        #endregion
    }
}