namespace eUseControl.BeekeepingStore.BusinessLogic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddOrderTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Orders",
                c => new
                {
                    OrderId = c.Int(nullable: false, identity: true),
                    UserId = c.Int(nullable: false),
                    OrderDate = c.DateTime(nullable: false),
                    OrderStatus = c.String(nullable: false, maxLength: 50),
                    TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    ShippingAddress = c.String(maxLength: 255),
                    BillingAddress = c.String(maxLength: 255),
                    PaymentMethod = c.String(maxLength: 50),
                    PaymentStatus = c.String(maxLength: 100),
                    ShippedDate = c.DateTime(),
                    TrackingNumber = c.String(maxLength: 100),
                    Notes = c.String(maxLength: 500),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "dbo.OrderItems",
                c => new
                {
                    OrderItemId = c.Int(nullable: false, identity: true),
                    OrderId = c.Int(nullable: false),
                    ProductId = c.Int(nullable: false),
                    ProductName = c.String(nullable: false, maxLength: 100),
                    UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Quantity = c.Int(nullable: false),
                    Subtotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.OrderItemId)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropTable("dbo.OrderItems");
            DropTable("dbo.Orders");
        }
    }
}