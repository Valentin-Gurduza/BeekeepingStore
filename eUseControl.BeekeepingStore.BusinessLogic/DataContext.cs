﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using eUseControl.BeekeepingStore.Domain.Entities.Product;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Domain.Entities.Payment;
using eUseControl.BeekeepingStore.Domain.Entities.Blog;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;

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
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogComment> BlogComments { get; set; }
    public DbSet<Promotion> Promotions { get; set; }

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

        // Configurare pentru BlogPost
        modelBuilder.Entity<BlogPost>()
            .HasKey(e => e.BlogPostId);

        // Configurare pentru BlogComment
        modelBuilder.Entity<BlogComment>()
            .HasKey(e => e.CommentId);

        // Configurare pentru relația dintre BlogPost și BlogComment
        modelBuilder.Entity<BlogComment>()
            .HasRequired(c => c.BlogPost)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.BlogPostId)
            .WillCascadeOnDelete(true);

        // Configurare pentru relația self-referencing în BlogComment pentru replies
        modelBuilder.Entity<BlogComment>()
            .HasOptional(c => c.ParentComment)
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .WillCascadeOnDelete(false);

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

        // Configurare pentru Payment
        modelBuilder.Entity<Payment>()
            .HasKey(p => p.PaymentId);

        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentMethod)
        .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
        .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .IsRequired();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        // Relația Order - Payment
        modelBuilder.Entity<Payment>()
            .HasRequired(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .WillCascadeOnDelete(false);

        // Configurare pentru Wishlist
        modelBuilder.Entity<Wishlist>()
            .HasKey(w => w.WishlistId);

        // Relația User - Wishlist
        modelBuilder.Entity<Wishlist>()
            .HasRequired(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .WillCascadeOnDelete(false);

        // Relația Product - Wishlist
        modelBuilder.Entity<Wishlist>()
            .HasRequired(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .WillCascadeOnDelete(false);

        // Configurare pentru Promotion
        modelBuilder.Entity<Promotion>()
            .HasKey(p => p.PromotionId);

        modelBuilder.Entity<Promotion>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Promotion>()
            .Property(p => p.PromotionType)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Promotion>()
            .Property(p => p.CouponCode)
            .HasMaxLength(50);

        modelBuilder.Entity<Promotion>()
            .Property(p => p.CustomerGroup)
            .HasMaxLength(50);

        // Relația Product - Promotion
        modelBuilder.Entity<Promotion>()
            .HasRequired(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .WillCascadeOnDelete(false);
    }
}
