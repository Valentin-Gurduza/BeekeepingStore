using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;

internal class DataContext : DbContext
{
    public DbSet<Session> Sessions { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<User> Users { get; set; }

    public DataContext() : base("name=BeekeepingStoreDB")
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}