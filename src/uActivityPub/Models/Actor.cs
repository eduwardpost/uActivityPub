namespace uActivityPub.Models;

public class Actor : ActivityPubBase
{
    // ReSharper disable once MemberCanBePrivate.Global
    public string PreferredUsername { get; set; } = default!;

    // ReSharper disable once UnusedMember.Global
    public string Name => PreferredUsername;

    public string? Url { get; set; }
    public string Inbox { get; set; } = default!;
    public string? Outbox { get; set; }
    public string? Followers { get; set; }
    public Icon? Icon { get; set; }
    public DateTime? Published { get; set; } = new DateTime(2024, 01, 1, 12, 00, 00, DateTimeKind.Utc);
    public DateTime? Updated { get; set; } = DateTime.Now;
    public bool ManuallyApprovesFollowers { get; set; } = false;
    public bool Discoverable { get; set; } = true;
    public bool Indexable { get; set; } = true;
    public string Summery { get; set; } = String.Empty;

    public PublicKey? PublicKey { get; set; }

    /// <summary>
    /// Create an empty actor
    /// </summary>
    public Actor()
    {
        Type = "Person";
        Context = new[]
        {
            "https://www.w3.org/ns/activitystreams",
            "https://w3id.org/security/v1"
        };
    }
}