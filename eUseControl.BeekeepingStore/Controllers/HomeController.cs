using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.Models;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Products()
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Beekeeping Kit",
                    Image = "~/Views/products/beehive.jpg",

                    Description = "Everything you need to get started with beekeeping.",
                    Price = 99.99m,
                    Category = "Kits"
                },
                new Product
                {
                    Name = "Protective Gear",
                    Image = "~/Content/images/products/protectivegear.png",
                    Description = "Stay safe while working with your bees.",
                    Price = 49.99m,
                    Category = "Gear"
                },
                new Product
                {
                    Name = "Honey Extractor",
                    Image = "~/Content/images/products/extractorrforhoney.jpg",
                    Description = "Tools and equipment for extracting honey.",
                    Price = 199.99m,
                    Category = "Equipment"
                },
                new Product
                {
                    Name = "Bee Hive",
                    Image = "~/Content/images/products/beehive.jpg",
                    Description = "A high-quality bee hive for your bees.",
                    Price = 149.99m,
                    Category = "Hives"
                },
                new Product
                {
                    Name = "Bee Smoker",
                    Image = "~/Content/images/products/beesmoker.jpg",
                    Description = "A smoker to calm your bees.",
                    Price = 29.99m,
                    Category = "Tools"
                },
                new Product
                {
                    Name = "Bee Feeder",
                    Image = "~/Content/images/products//beefeeder.png",
                    Description = "A feeder to provide food for your bees.",
                    Price = 19.99m,
                    Category = "Tools"
                }
            };
            return View(products);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Blog()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
    }
}