using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public sealed class MessageHolder<TSource> : IMessageHolder<TSource>
{
    public MessageHolder()
    {
    }

    // ReSharper disable once UnusedMember.Global
    public MessageHolder(string? topic = null) : this(Enumerable.Empty<TSource>(), topic)
    {
    }

    public MessageHolder(IEnumerable<TSource> source, string? topic = null)
    {
        Source = source.ToList();
        Topic = topic ?? typeof(TSource).GetTopicName();
    }

    public IList<TSource>? Source { get; init; }
    public string Topic { get; set; } = null!;

    public DateTime PublishUtc { get; set; }
    public TransitStatus TransitStatus { get; set; } = TransitStatus.Unprocessed;
    public DateTime? ErrorUtc { get; set; }
}

public sealed class MessageHolder<TSource, TMapped> : IMessageHolder<TSource, TMapped>
{
    private string? _payoffType;
    private string? _sourceType;
    public string Subject { get; set; } = null!;
    public MessageHolderError? Error { get; set; }
    public bool HasError => Error != null;
    public TMapped? Payoff { get; set; }

    public string? TopicType
    {
        get => _sourceType ??= typeof(TSource).FullName;
        set => _sourceType = value;
    }

    public string? SubjectType
    {
        get => _payoffType ??= typeof(TMapped).FullName;
        set => _payoffType = value;
    }

    public string? VirtualHost { get; set; }
    public IList<TSource>? Source { get; init; }
    public string Topic { get; set; } = null!;
    public DateTime PublishUtc { get; set; }
    public TransitStatus TransitStatus { get; set; }
    public DateTime? ErrorUtc { get; set; }

    public string? GetResult()
    {
        return HasError ? Error?.Message : TransitStatus.ToString();
    }

    public void EnsureMappingIsComplete()
    {
        if (Payoff is null)
            throw new MappedObjectNotAssignedException();
    }
}