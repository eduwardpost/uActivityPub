namespace uActivityPub.Models;

public class Note : ActivityPubBase
{
    public string Content { get; set; } = default!;
    public string? Url { get; set; }
    public string? Published { get; set; }
    public string? AttributedTo { get; set; }
    public string? To { get; set; }

    public Note()
    {
        Context = new[]
        {
            "https://www.w3.org/ns/activitystreams"
        };
        Type = "Note";
    }
}