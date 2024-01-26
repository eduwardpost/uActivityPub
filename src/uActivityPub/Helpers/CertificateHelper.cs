using System.Security.Cryptography;

namespace uActivityPub.Helpers;

public static class CertificateHelper
{
    // ReSharper disable once InconsistentNaming
    public static string ExportRSAPublicKeyPem(this RSA certificate)
    {
        var exported = certificate.ExportRSAPublicKey();
        return new string(PemEncoding.Write("RSA PUBLIC KEY", exported));
    }

    // ReSharper disable once InconsistentNaming
    public static RSA GetRSAFromPem(this string pem, int keySize = 2048)
    {
        var rsa = RSA.Create(keySize);
        rsa.ImportFromPem(pem);
        return rsa;
    }

    // ReSharper disable once InconsistentNaming
    public static string ExportRSAPrivateKeyPem(this RSA certificate)
    {
        var exported = certificate.ExportRSAPrivateKey();
        return new string(PemEncoding.Write("RSA PRIVATE KEY", exported));
    }
}