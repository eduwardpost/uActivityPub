using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Services;

public class SignatureService : ISignatureService
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;

    public SignatureService(
        IUmbracoDatabaseFactory  databaseFactory,
        IHttpClientFactory httpClientFactory,
        IOptions<WebRoutingSettings> webRoutingSettings)
    {
        _databaseFactory = databaseFactory;
        _httpClientFactory = httpClientFactory;
        _webRoutingSettings = webRoutingSettings;
    }

    public async Task<Actor?> GetActor(string actorUrl)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.GetAsync(actorUrl);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseBody = await response.Content.ReadAsStringAsync();
        var actor = JsonConvert.DeserializeObject<Actor>(responseBody);

        return actor;
    }

    public async Task<(string KeyId, RSA Rsa)> GetPrimaryKeyForUser(IUser user)
    {
        var database = _databaseFactory.CreateDatabase();
        
        var userKey = await database.FirstOrDefaultAsync<UserKeysSchema>("SELECT * FROM userKeys WHERE UserId = @0", user.Id);
        var rsa = RSA.Create(2048);
        
        if (userKey == null)
        {
            //user does not have pub private key yet, lets make a pair

            userKey = new UserKeysSchema
            {
                Id = user.Id,
                PublicKey = rsa.ExportRSAPublicKeyPem(),
                PrivateKey = rsa.ExportRSAPrivateKeyPem()
            };

            database.Insert(userKey);
        }
        else
        {
            rsa = userKey.PrivateKey.GetRSAFromPem();
        }
        
        return ($"{_webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{user.ActivityPubUserName()}#main-key", rsa);
    }
}