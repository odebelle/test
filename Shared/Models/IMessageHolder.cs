using Shared.Enums;

namespace Shared.Models;

public interface IMessageHolder<TSource>
{
    IList<TSource>? Source { get; init; }
    string Topic { get; set; }
    DateTime PublishUtc { get; set; }
    TransitStatus TransitStatus { get; set; }
    public DateTime? ErrorUtc { get; set; }
}

public interface IMessageHolder
{
    string Id { get; set; }
    IList<object>? Source { get; init; }
    string Topic { get; set; }
    string Subject { get; set; }
    MessageHolderError? Error { get; set; }
    bool HasError { get; }
    object? Payoff { get; set; }
    DateTime PublishUtc { get; set; }
    TransitStatus TransitStatus { get; set; }
    string? TopicType { get; set; }
    string? SubjectType { get; set; }
    string? VirtualHost { get; set; }
    public DateTime? ErrorUtc { get; set; }
    public DateTime? DeadLetterUtc { get; set; }
}

public interface IMessageHolder<TSource, TMapped> : IMessageHolder<TSource>
{
    string Subject { get; set; }
    MessageHolderError? Error { get; set; }
    bool HasError { get; }
    TMapped? Payoff { get; set; }
    string? TopicType { get; set; }
    string? SubjectType { get; set; }
    string? VirtualHost { get; set; }
}