using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared;

public class BrokerOptions:Dictionary<string, object>
{
    
}
[JsonConverter(typeof(SnakeCaseEnumConverter<AckMode>))]
public enum AckMode
{
    AckRequeueTrue,
    AckRequeueFalse,
}

public class SnakeCaseEnumConverter<T> : JsonConverter<T> where T: struct, IConvertible
{
    private static string ToPascalCase(string str)
    {
        // get all underscore index
        if (!str.StartsWith('_'))
            str = str.Insert(0, "_");

        var indexes = str.Select((x, i) => x == '_' ? i - 1 : 0).Distinct();

        // remove underscore
        var result = str.Replace("_", string.Empty);
        // to upper at index
        result = string.Concat(result.Select((x, i) => indexes.Contains(i) ? char.ToUpper(x) : x));
        return result;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var parsed = Enum.TryParse<T>(ToPascalCase(reader.GetString()!), out var result);
        
        return parsed ? result : default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var s = value.ToString();
        if (s == null) 
            return;
        
        var result= string.Concat(s.Select((x, i) => i > 0 && char.IsUpper(x) ? $"_{x}" : $"{x}")).ToLower();
        writer.WriteStringValue(result);
    }
}

[JsonConverter(typeof(SnakeCaseEnumConverter<QueueEncoding>))]
public enum QueueEncoding
{
    Auto
}

public class QueueDetailsBodyRequest
{
    [JsonPropertyName("count")] public int Count { get; set; } = 1;
    [JsonPropertyName("ackmode")] public AckMode AckMode { get; set; } = AckMode.AckRequeueTrue;
    [JsonPropertyName("encoding")] public QueueEncoding Encoding { get; set; } = QueueEncoding.Auto;
    [JsonPropertyName("truncate")] public int? Truncate { get; set; }

    [JsonIgnore] public QueueDetailsQueryParameters QueryParameters { get; set; } = new();
}
public class QueueDetailsQueryParameters
{
    [JsonPropertyName("lengths_age")] public int LengthsAge { get; set; } = 3600;
    [JsonPropertyName("lengths_incr")] public int LengthsIncr { get; set; } = 60;
}

public class QueueDetail
{
    
}