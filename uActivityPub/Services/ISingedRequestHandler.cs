using System.Security.Cryptography;

namespace uActivityPub.Services;

public interface ISingedRequestHandler
{
    Task<HttpResponseMessage> SendSingedPost(Uri requestUri, RSA rsa, string jsonBody, string keyId);
}