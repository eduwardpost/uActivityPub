using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core.Models.Membership;

namespace uActivityPub.Helpers;

public static class GravatarHelper
{
    public static string GetGravatarUrl(this IUser user)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(user.Email);
        var hash = md5.ComputeHash(inputBytes);

        // convert byte array to hex string
        var sb = new StringBuilder();
        foreach (var @byte in hash)
        {
            sb.Append(@byte.ToString("X2"));
        }

        return $"https://www.gravatar.com/avatar/{sb}".ToLowerInvariant();
    }
}