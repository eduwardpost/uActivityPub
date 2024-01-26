namespace uActivityPub.Models;

public class Icon
{
    public string Type { get; set; } = "Image";
    public string MediaType { get; set; } = "image/png";
    public string Url { get; set; } = default!;
}