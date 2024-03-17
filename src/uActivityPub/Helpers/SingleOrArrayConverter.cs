using System.Text.Json;
using System.Text.Json.Serialization;

namespace uActivityPub.Helpers;

public class SingleOrArrayConverter<TEnumeratingObject> : JsonConverter<IEnumerable<TEnumeratingObject>> 
{

    public override IEnumerable<TEnumeratingObject>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        if(reader.TokenType == JsonTokenType.StartArray) {
            return JsonSerializer.Deserialize<List<TEnumeratingObject>>(ref reader, options);
        }
        return new[] { JsonSerializer.Deserialize<TEnumeratingObject>(ref reader, options)! };
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<TEnumeratingObject>? value, JsonSerializerOptions? options) {
        if(value != null)
        {
            var items = value.ToList();
            
            if(items.Count == 1) {
                JsonSerializer.Serialize(writer, items[0], items[0]?.GetType() ?? throw new InvalidOperationException(), options);
            }
            else {
                JsonSerializer.Serialize(writer, items, items.GetType(), options);
            }
        } else throw new NotSupportedException();
    }
}