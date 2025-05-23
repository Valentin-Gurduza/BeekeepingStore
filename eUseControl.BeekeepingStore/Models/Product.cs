﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eUseControl.BeekeepingStore.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
