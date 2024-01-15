using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.Elasticsearch;
using Shared.Clients;
using Shared.Models;

namespace Dispatcher;

public static class DispatcherExtensions
{
    /// <summary>
    /// GetStringUTF8
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string GetStringUtf8(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// GetBytesUTF8
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static byte[] GetBytesUtf8(this string s)
    {
        return Encoding.UTF8.GetBytes(s);
    }

    public static string GetTopicName(this Type o)
    {
        return o.FullName!;
    }
}