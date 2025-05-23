﻿namespace eUseControl.BeekeepingStore.Domain.Enums
{
    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
        public const string Returned = "Returned";
    }

    public static class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Refunded = "Refunded";
    }
}