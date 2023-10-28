using System.Security.Cryptography;

namespace uActivityPub.Services;

public interface ISingedRequestHandler
{
    /// <summary>
    /// Send a request singed with the http-signatures protocol (IETF draft-cavage-http-signatures-12)
    /// </summary>
    /// <param name="requestUri">The URI the request is to be sent towards</param>
    /// <param name="rsa">the RSA key used in the singing</param>
    /// <param name="jsonBody">The JSON body to be send, serialized</param>
    /// <param name="keyId">They id of the key</param>
    /// <returns>The response on the request</returns>
    Task<HttpResponseMessage> SendSingedPost(Uri requestUri, RSA rsa, string jsonBody, string keyId);
}