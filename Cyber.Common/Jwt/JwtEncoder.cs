using JWT.Algorithms;
using JWT.Serializers;
using Cyber;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static JWT.Internal.EncodingHelper;

namespace JWT
{
    /// <summary>
    /// Encodes Jwt.
    /// </summary>
    public sealed class JwtEncoder : IJwtEncoder
    {
        private readonly IAlgorithmFactory _algFactory;
        private readonly IBase64UrlEncoder _urlEncoder;

        /// <summary>
        /// Creates an instance of <see cref="JwtEncoder" />
        /// </summary>
        /// <param name="algFactory">The JWT algorithm factory</param>
        /// <param name="urlEncoder">The base64 URL encoder</param>
        public JwtEncoder(IAlgorithmFactory algFactory, IBase64UrlEncoder urlEncoder)
        {
            _algFactory = algFactory;
            _urlEncoder = urlEncoder;
        }

        /// <summary>
        /// Creates an instance of <see cref="JwtEncoder" />
        /// </summary>
        /// <param name="algorithm">The JWT algorithm</param>
        /// <param name="urlEncoder">The base64 URL encoder</param>
        public JwtEncoder(IJwtAlgorithm algorithm, IBase64UrlEncoder urlEncoder)
            : this(new DelegateAlgorithmFactory(algorithm), urlEncoder)
        {
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException" />
        public string Encode<TPayload>(IDictionary<string, object> extraHeaders, TPayload payload, JsonTypeInfo<TPayload> payloadTypeInfo, byte[] key)
        {
            if (payload is null)
                throw new ArgumentNullException(nameof(payload));

            var algorithm = _algFactory.Create(null);
            if (algorithm is null)
                throw new ArgumentNullException(nameof(algorithm));
            if (!algorithm.IsAsymmetric() && key is null && algorithm is not NoneAlgorithm)
                throw new ArgumentNullException(nameof(key));

            var header = extraHeaders is null ?
                new Dictionary<string, object>(2, StringComparer.OrdinalIgnoreCase) :
                new Dictionary<string, object>(extraHeaders, StringComparer.OrdinalIgnoreCase);

            if (!header.ContainsKey("typ"))
            {
                header.Add("typ", "JWT");
            }
            header.Add("alg", algorithm.Name);

            var headerBytes = GetBytes(JsonSerializer.Serialize(header, JsonTypeInfoShared.IDictTypeInfo));
            var payloadBytes = GetBytes(JsonSerializer.Serialize(payload, payloadTypeInfo));

            var headerSegment = _urlEncoder.Encode(headerBytes);
            var payloadSegment = _urlEncoder.Encode(payloadBytes);

            var stringToSign = headerSegment + "." + payloadSegment;
            var bytesToSign = GetBytes(stringToSign);

            var signatureSegment = GetSignatureSegment(algorithm, key, bytesToSign);
            return stringToSign + "." + signatureSegment;
        }

        private string GetSignatureSegment(IJwtAlgorithm algorithm, byte[] key, byte[] bytesToSign)
        {
            switch (algorithm)
            {
                case NoneAlgorithm _:
                {
                    return null;
                }
                default:
                {
                    var signature = algorithm.Sign(key, bytesToSign);
                    return _urlEncoder.Encode(signature);
                }
            }
        }
    }
}
