namespace uActivityPub.Models;

public class Activity : ActivityPubBase
{
    public string Actor { get; set; } = default!;
    public object Object { get; set; } = null!;
    
    public Activity()
    {
        Context = new[]
        {
            "https://www.w3.org/ns/activitystreams"
        };
    }
}