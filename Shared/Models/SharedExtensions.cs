using System.Text;
using System.Text.Json;

namespace Shared.Models;

public static class SharedExtensions
{
    private static readonly Dictionary<string, Type> StoredTypes = new();

    private static readonly JsonSerializerOptions JsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true
    };
    
    /// <summary>
    /// GetStringUTF8
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string GetStringUtf8(this ReadOnlyMemory<byte> bytes)
    {
        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Extract Json text from objects
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static string ToJson(this object? instance)
    {
        return ToJson(instance, JsonSerializerOptions);
    }

    /// <summary>
    /// Extract Json text from object with options.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ToJson(this object? instance, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(instance, options);
    }

    /// <summary>
    /// Extract Json text from objects
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static byte[] SerializeToUtf8Bytes(this object? instance)
    {
        return SerializeToUtf8Bytes(instance, JsonSerializerOptions);
    }

    /// <summary>
    /// Extract Json text from object with options.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static byte[] SerializeToUtf8Bytes(this object? instance, JsonSerializerOptions options)
    {
        return JsonSerializer.SerializeToUtf8Bytes(instance, options);
    }
    
    public static Type GetTypeFromAssemblies(this string? type)
    {
        if (type is null)
            throw new NullReferenceException("type is not defined");

        if (StoredTypes.TryGetValue(type, out var result)) 
            return result;
        
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            result = assembly.GetType(type);
            
            if (result is null) 
                continue;
            
            StoredTypes.TryAdd(type, result);
            return result;
        }
        throw new NullReferenceException("Unable to determine type.");

    }
}