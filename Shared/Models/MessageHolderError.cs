namespace Shared.Models;

public class MessageHolderError
{
    public MessageHolderError()
    {
        
    }
    public MessageHolderError(Exception exception)
    {
        Message = exception.Message;
        Source = exception.Source;
        HelpLink = exception.HelpLink;
        StackTrace = exception.StackTrace;
        Hresult = exception.HResult;
    }

    public string? HelpLink { get; set; }

    public string? StackTrace { get; set; }

    public int Hresult { get; set; }

    public string? Source { get; set; }

    public string Message { get; set; } = null!;
}