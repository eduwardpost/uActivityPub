namespace uActivityPub.Models;

public class OrderedCollection : Collection
{
    public OrderedCollection()
    {
        Type = "OrderedCollection";
    }
}



public class OrderedCollection<T> : Collection<T>
{
}