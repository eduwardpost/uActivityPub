using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core.Models.Membership;

namespace uActivityPub.Helpers;

/// <summary>
/// Helper class to get gravatar url's
/// </summary>
public static class GravatarHelper
{
    /// <summary>
    /// Get a gravatar link based on the email form the umbraco user account
    /// </summary>
    /// <param name="user">The umbraco user account to get the gravatar url from</param>
    /// <returns>Gravatar url</returns>
    public static string GetGravatarUrl(this IUser user)
    {
        return GetUrl(user.Email);
    }

    /// <summary>
    /// Get a gravatar link based on the email address given
    /// </summary>
    /// <param name="email">The email to get the gravatar url from</param>
    /// <returns>Gravatar url</returns>
    public static string GetGravatarUrl(this string email)
    {
        return GetUrl(email);
    }

    private static string GetUrl(string email)
    {
        var inputBytes = Encoding.ASCII.GetBytes(email);
        var hash = MD5.HashData(inputBytes);

        // convert byte array to hex string
        var sb = new StringBuilder();
        foreach (var @byte in hash)
        {
            sb.Append(@byte.ToString("X2"));
        }

        return $"https://www.gravatar.com/avatar/{sb}".ToLowerInvariant();
    }
}