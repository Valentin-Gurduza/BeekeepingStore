using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using eUseControl.BeekeepingStore.Domain.Entities.Product;
using eUseControl.BeekeepingStore.Domain.Entities.Order;

internal class DataContext : DbContext
{
    public DbSet<Session> Sessions { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UDBTable> UDBTables { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public DataContext() : base("eUseControl.BeekeepingStore")
    {
        Debug.WriteLine("DataContext constructor called with connection string name: eUseControl.BeekeepingStore");
        try
        {
            // Afișează stringul de conexiune complet pentru debugging
            Debug.WriteLine("Connection string: " + this.Database.Connection.ConnectionString);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error accessing connection string: " + ex.Message);
        }

        // Activez log-ul pentru a vedea query-urile SQL
        this.Database.Log = s => Debug.WriteLine(s);

        // Dezactivează lazy loading și change tracking pentru performanță
        this.Configuration.LazyLoadingEnabled = false;

        // Setarea timeout-ului mai mare pentru operațiuni
        this.Database.CommandTimeout = 180; // 3 minute
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        Debug.WriteLine("DataContext.OnModelCreating called");
        base.OnModelCreating(modelBuilder);

        // Configurare explicită pentru UDBTable
        modelBuilder.Entity<UDBTable>()
            .ToTable("UDBTables")
            .HasKey(e => e.Id);

        modelBuilder.Entity<UDBTable>()
            .Property(e => e.UserName)
            .HasColumnName("Username")
            .IsRequired()
            .HasMaxLength(30);

        modelBuilder.Entity<UDBTable>()
            .Property(e => e.Password)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<UDBTable>()
            .Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<UDBTable>()
            .Property(e => e.Last_Login)
            .IsRequired();

        modelBuilder.Entity<UDBTable>()
            .Property(e => e.UserIp)
            .IsRequired()
            .HasMaxLength(60);

        modelBuilder.Entity<UDBTable>()
            .Property(e => e.Level)
            .IsRequired();

        // Configurare pentru Session
        modelBuilder.Entity<Session>()
            .HasKey(e => e.SessionPK);

        // Configurare pentru ErrorLog
        modelBuilder.Entity<ErrorLog>()
            .HasKey(e => e.ErrorId);

        // Configurare pentru UserActivity
        modelBuilder.Entity<UserActivity>()
            .HasKey(e => e.ActivityId);

        // Configurare pentru User
        modelBuilder.Entity<User>()
            .HasKey(e => e.UserId);

        modelBuilder.Entity<User>()
            .Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(e => e.Password)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(e => e.UserIp)
            .HasMaxLength(60);

        // Configurare pentru Product
        modelBuilder.Entity<Product>()
            .HasKey(e => e.ProductId);

        modelBuilder.Entity<Product>()
            .Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Product>()
            .Property(e => e.Price)
            .IsRequired();

        // Configurare pentru Order
        modelBuilder.Entity<Order>()
            .HasKey(e => e.OrderId);

        modelBuilder.Entity<Order>()
            .Property(e => e.OrderStatus)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Order>()
            .Property(e => e.TotalAmount)
            .IsRequired();

        // Configurare pentru OrderItem
        modelBuilder.Entity<OrderItem>()
            .HasKey(e => e.OrderItemId);

        modelBuilder.Entity<OrderItem>()
            .Property(e => e.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<OrderItem>()
            .Property(e => e.UnitPrice)
            .IsRequired();

        modelBuilder.Entity<OrderItem>()
            .Property(e => e.Quantity)
            .IsRequired();

        // Relația Order - OrderItem
        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithRequired(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .WillCascadeOnDelete(true);
    }
}
