using System;

internal class DataContext : IDisposable
{

    public object Sessions { get; internal set; }
    public object ErrorLogs { get; internal set; }
    public object UserActivities { get; internal set; }
    public object Users { get; internal set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    internal void SaveChanges()
    {
        throw new NotImplementedException();
    }
}