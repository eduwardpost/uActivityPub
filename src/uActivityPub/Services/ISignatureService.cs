using System.Security.Cryptography;
using uActivityPub.Models;

namespace uActivityPub.Services;

public interface ISignatureService
{
    Task<Actor?> GetActor(string actorUrl);
    Task<(string KeyId, RSA Rsa)> GetPrimaryKeyForUser(string userName, int userId);
}