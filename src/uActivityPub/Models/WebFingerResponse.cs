namespace uActivityPub.Models;

public class WebFingerResponse
{
    public string Subject { get; set; } = default!;
    public WebFingerLink[] Links { get; set; } = Array.Empty<WebFingerLink>();
}

public class WebFingerLink
{
    public string Rel { get; set; } = default!;
    public string Type { get; set; } = "application/activity+json";
    public string Href { get; set; } = default!;
}