namespace uActivityPub.Models;

public class PublicKey
{
    public string Id { get; set; } = default!;
    public string Owner { get; set; } = default!;
    public string PublicKeyPem { get; set; } = default!;
}