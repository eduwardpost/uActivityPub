namespace uActivityPub.Models;

public class Collection: ActivityPubBase
{
    public int TotalItems { get; set; } = 0;
    
    public Collection()
    {
        Context = new[]
        {
            "https://www.w3.org/ns/activitystreams"
        };
        Type = "Collection";
        
    }
}

public class Collection<T> : Collection
{
    public List<T> Items { get; set; } = [];
}