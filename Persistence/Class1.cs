using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Persistence;

public static class WorkerSettingExceptionExtension
{
    public static void IsCorrectlySetForWorker(this string property, [CallerMemberName] string name = "")
    {
        if (string.IsNullOrEmpty(property))
            throw new WorkerSettingException($"{name} is missing.");
    }
}

public class Producer
{
    [Key] public int DispatchId { get; set; }
    public string Topic { get; set; } = null!;
    public string Cron { get; set; } = null!;
    public DateTime? FirstRun { get; set; }
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? LastSuccess { get; set; }
    public DateTime? LastError { get; set; }
    public string? Information { get; set; }

    public Dispatch? Dispatch { get; set; }

    public void EnsureIsCorrectlySet()
    {
        Topic.IsCorrectlySetForWorker();
        Cron.IsCorrectlySetForWorker();
    }
}
public class Dispatch
{
    private string _name = null!;
    private const string CatchAll = "{**catch-all}";
    public int DispatchId { get; set; }

    public string Name
    {
        get => _name;
        set => _name = value.ToLower();
    }

    public string Cluster { get; set; } = null!;
    public string? Subject { get; set; }
    public bool Enabled { get; set; }
    public bool? IsWorker { get; set; }

    public Producer? Producer { get; set; }
    public Consumer? Consumer { get; set; }

    [JsonIgnore]
    public string Match => $"{Name}/{CatchAll}";
    [JsonIgnore]
    public string RemovePrefix => $"/{Name}";

    public void EnsureIsCorrectlySetForWorker()
    {
        if (IsWorker != true)
            throw new WorkerSettingException("No worker was defined.");

        Producer?.EnsureIsCorrectlySet();

        Consumer?.EnsureIsCorrectlySet();
    }
}

public class Consumer
{
    [Key] public int DispatchId { get; set; }
    public string Topic { get; set; } = null!;
    public string? Marshal { get; set; }

    public DateTime? FirstRun { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? LastSuccess { get; set; }
    public DateTime? LastError { get; set; }
    public string? Information { get; set; }
    public Dispatch? Dispatch { get; set; }

    public void EnsureIsCorrectlySet()
    {
        Topic.IsCorrectlySetForWorker();
    }
}

public class Element
{
    [Key]
    public string Topic { get; set; } = null!;
    public string? Cron { get; set; } 
    public string? Marshal { get; set; }
    public DateTime? FirstRun { get; set; }
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? LastSuccess { get; set; }
    public DateTime? LastError { get; set; }
    public string? Information { get; set; }
}