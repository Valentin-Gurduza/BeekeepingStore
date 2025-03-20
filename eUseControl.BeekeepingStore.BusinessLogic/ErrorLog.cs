using System;

internal class ErrorLog
{
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public DateTime CreatedAt { get; set; }
}