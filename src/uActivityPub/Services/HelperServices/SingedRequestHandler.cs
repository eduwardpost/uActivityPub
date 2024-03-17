using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace uActivityPub.Services.HelperServices;

public class SingedRequestHandler : ISingedRequestHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SingedRequestHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    /// <inheritdoc/>
    public async Task<HttpResponseMessage> SendSingedPost(Uri requestUri, RSA rsa, string jsonBody, string keyId)
    {
        var client = _httpClientFactory.CreateClient();
        
        var request = new HttpRequestMessage
        {
            RequestUri = requestUri,
            Method = HttpMethod.Post,
            Content = new StringContent(jsonBody, Encoding.Default, "application/activity+json")
        };
        
        // Specify the 'x-ms-date' header as the current UTC timestamp according to the RFC1123 standard
        var date = DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture);
        // Compute a content hash for the 'x-ms-content-sha256' header.
        var contentHash = $"SHA-256={ComputeContentHash(jsonBody)}";

        // Prepare a string to sign.
        var stringToSign = $"(request-target): {request.Method.Method.ToLowerInvariant()} {requestUri.AbsolutePath.ToLowerInvariant()}\nhost: {requestUri.Host.ToLowerInvariant()}\ndate: {date}\ndigest: {contentHash}";
        // Compute the signature.
        var signature = ComputeSignature(stringToSign, rsa);
        // Concatenate the string, which will be used in the authorization header.
        var authorizationHeader = $"keyId=\"{keyId}\",headers=\"(request-target) host date digest\",signature=\"{signature}\"";

        // Add a Date header.
        request.Headers.Add("Date", date);
        // Add a Digest header.
        request.Headers.Add("Digest", contentHash);
        // Add a Signature header.
        request.Headers.Add("Signature", authorizationHeader);
        
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

        return await client.SendAsync(request);
    }
    
    private static string ComputeContentHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashedBytes);
    }
    
    private static string ComputeSignature(string stringToSign, RSA rsa)
    {
        var bytes = Encoding.ASCII.GetBytes(stringToSign);
        var hash = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(hash);
    }
}