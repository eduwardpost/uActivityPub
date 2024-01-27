using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Services;

public class SignatureService(
    IUmbracoDatabaseFactory databaseFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<WebRoutingSettings> webRoutingSettings)
    : ISignatureService
{
    public async Task<Actor?> GetActor(string actorUrl)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.GetAsync(actorUrl);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseBody = await response.Content.ReadAsStringAsync();
        var actor = JsonConvert.DeserializeObject<Actor>(responseBody);

        return actor;
    }

    public async Task<(string KeyId, RSA Rsa)> GetPrimaryKeyForUser(string userName, int userId)
    {
        var database = databaseFactory.CreateDatabase();
        
        var userKey = await database.FirstOrDefaultAsync<UserKeysSchema>("SELECT * FROM userKeys WHERE UserId = @0", userId);
        var rsa = RSA.Create(2048);
        
        if (userKey == null)
        {
            //user does not have pub private key yet, lets make a pair

            userKey = new UserKeysSchema
            {
                Id = userId,
                PublicKey = rsa.ExportRSAPublicKeyPem(),
                PrivateKey = rsa.ExportRSAPrivateKeyPem()
            };

            database.Insert(userKey);
        }
        else
        {
            rsa = userKey.PrivateKey.GetRSAFromPem();
        }
        
        return ($"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{userName}#main-key", rsa);
    }
}