using System.Text.Json;
using Shared.Enums;

namespace Shared.Models;

public sealed class MessageHolder : IMessageHolder
{
    public MessageHolder()
    {
    }

    private MessageHolder(IList<object>? source, string topic, string subject, MessageHolderError? error,
        object? payoff,
        DateTime publishUtc, TransitStatus transitStatus, string? topicType, string? subjectType, string? virtualHost,
        DateTime? errorUtc)
    {
        Id = Guid.NewGuid().ToString();
        Source = source;
        Topic = topic;
        Subject = subject;
        Error = error;
        Payoff = payoff;
        PublishUtc = publishUtc;
        TransitStatus = transitStatus;
        TopicType = topicType;
        SubjectType = subjectType;
        VirtualHost = virtualHost;
        ErrorUtc = errorUtc;
        DeadLetterUtc = DateTime.UtcNow;
    }

    public string Id { get; set; } = null!;
    public IList<object>? Source { get; init; }
    public string Topic { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public MessageHolderError? Error { get; set; }
    public bool HasError => Error is not null;
    public object? Payoff { get; set; }
    public DateTime PublishUtc { get; set; }
    public TransitStatus TransitStatus { get; set; }
    public string? TopicType { get; set; }
    public string? SubjectType { get; set; }
    public string? VirtualHost { get; set; }
    public DateTime? ErrorUtc { get; set; }
    public DateTime? DeadLetterUtc { get; set; }

    public static IMessageHolder Factory<TSource, TMapped>(IMessageHolder<TSource, TMapped> m)
        where TSource : class where TMapped : new()
    {
        IList<object>? source = null;

        if (m.Source != null)
        {
            source = new List<object>(m.Source);
        }

        return new MessageHolder(source, m.Topic, m.Subject, m.Error, m.Payoff, m.PublishUtc, m.TransitStatus,
            m.TopicType, m.SubjectType, m.VirtualHost, m.ErrorUtc);
    }

    public void FixProperties()
    {
        if (Payoff is JsonElement payoff)
            Payoff = payoff.Deserialize(SubjectType.GetTypeFromAssemblies());

        if (Source is null || !Source.Any() || Source.First() is not JsonElement) 
            return;
        
        var sources = new List<object>();
        
        foreach (var source in Source)
        {
            if ((JsonElement?)source is not { } src) 
                continue;
            var item = src.Deserialize(TopicType.GetTypeFromAssemblies());
            if(item is not null)sources.Add(item);
        }

        Source.Clear();

        foreach (var source in sources)
            Source.Add(source);
    }
}