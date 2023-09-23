using System.Security.Cryptography;
using uActivityPub.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace uActivityPub.Services;

public interface ISignatureService
{
    Task<Actor?> GetActor(string actorUrl);
    Task<(string KeyId, RSA Rsa)> GetPrimaryKeyForUser(IUser user);
}