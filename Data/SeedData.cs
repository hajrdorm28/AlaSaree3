using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AlaSaree3.Models;

namespace AlaSaree3.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            string[] roles = new[] { "Admin", "Seller", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = configuration["AdminUserSeed:Email"] ?? "ZiadWael@gmail.com";
            var adminPassword = configuration["AdminUserSeed:Password"] ?? "ziad@Password123!";
            var adminFullName = configuration["AdminUserSeed:FullName"] ?? "System Administrator";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Sample Sellers
            var seller1Email = "techstore@alasaree3.com";
            var seller1 = await userManager.FindByEmailAsync(seller1Email);
            if (seller1 == null)
            {
                seller1 = new ApplicationUser
                {
                    UserName = seller1Email,
                    Email = seller1Email,
                    FullName = "TechZone Official Store",
                    PhoneNumber = "+1-555-0101",
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                };
                var res = await userManager.CreateAsync(seller1, "Seller@123456!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(seller1, "Seller");
                }
            }

            var seller2Email = "fashionhub@alasaree3.com";
            var seller2 = await userManager.FindByEmailAsync(seller2Email);
            if (seller2 == null)
            {
                seller2 = new ApplicationUser
                {
                    UserName = seller2Email,
                    Email = seller2Email,
                    FullName = "Urban Trends Boutique",
                    PhoneNumber = "+1-555-0102",
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                };
                var res = await userManager.CreateAsync(seller2, "Seller@123456!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(seller2, "Seller");
                }
            }

            // 4. Seed Sample Customers
            var customer1Email = "customer1@alasaree3.com";
            var customer1 = await userManager.FindByEmailAsync(customer1Email);
            if (customer1 == null)
            {
                customer1 = new ApplicationUser
                {
                    UserName = customer1Email,
                    Email = customer1Email,
                    FullName = "Alexander Wright",
                    PhoneNumber = "+1-555-0201",
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                };
                var res = await userManager.CreateAsync(customer1, "Customer@123456!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(customer1, "Customer");
                }
            }

            var customer2Email = "customer2@alasaree3.com";
            var customer2 = await userManager.FindByEmailAsync(customer2Email);
            if (customer2 == null)
            {
                customer2 = new ApplicationUser
                {
                    UserName = customer2Email,
                    Email = customer2Email,
                    FullName = "Sophia Martinez",
                    PhoneNumber = "+1-555-0202",
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                };
                var res = await userManager.CreateAsync(customer2, "Customer@123456!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(customer2, "Customer");
                }
            }

            // 5. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics & Gadgets", Description = "Smartphones, laptops, headphones, smartwatches, and premium accessories.", CreatedAt = DateTime.UtcNow.AddDays(-90) },
                    new Category { Name = "Fashion & Apparel", Description = "Trendy clothing, footwear, luxury bags, and designer accessories.", CreatedAt = DateTime.UtcNow.AddDays(-90) },
                    new Category { Name = "Home & Living", Description = "Modern furniture, kitchen appliances, home decor, and smart home lighting.", CreatedAt = DateTime.UtcNow.AddDays(-90) },
                    new Category { Name = "Books & Stationery", Description = "Academic textbooks, bestselling novels, journals, and art supplies.", CreatedAt = DateTime.UtcNow.AddDays(-90) },
                    new Category { Name = "Sports & Fitness", Description = "Gym equipment, athletic apparel, outdoor gear, and nutritional accessories.", CreatedAt = DateTime.UtcNow.AddDays(-90) }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 6. Seed Products
            if (!await context.Products.AnyAsync() && seller1 != null && seller2 != null)
            {
                var electronics = await context.Categories.FirstAsync(c => c.Name.Contains("Electronics"));
                var fashion = await context.Categories.FirstAsync(c => c.Name.Contains("Fashion"));
                var home = await context.Categories.FirstAsync(c => c.Name.Contains("Home"));
                var sports = await context.Categories.FirstAsync(c => c.Name.Contains("Sports"));

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Pro Wireless Noise-Cancelling Headphones",
                        Description = "Engineered with industry-leading active noise cancellation, 40-hour battery life, and crystal-clear acoustic fidelity with custom 40mm neodymium drivers.",
                        Price = 249.99m,
                        AvailableQuantity = 35,
                        CategoryId = electronics.Id,
                        SellerId = seller1.Id,
                        ImageUrl = "/images/products/wireless-headphones.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-25)
                    },
                    new Product
                    {
                        Name = "Ultra-Slim 4K HDR Monitor 27-Inch",
                        Description = "IPS panel with 99% sRGB color gamut, 144Hz refresh rate, USB-C 65W power delivery, and ultra-thin ergonomic aluminum bezels.",
                        Price = 389.50m,
                        AvailableQuantity = 18,
                        CategoryId = electronics.Id,
                        SellerId = seller1.Id,
                        ImageUrl = "/images/products/4k-monitor.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-22)
                    },
                    new Product
                    {
                        Name = "Mechanical RGB Gaming Keyboard",
                        Description = "Hot-swappable linear mechanical switches, aircraft-grade aluminum chassis, per-key RGB backlighting, and braided detachable cable.",
                        Price = 119.00m,
                        AvailableQuantity = 42,
                        CategoryId = electronics.Id,
                        SellerId = seller1.Id,
                        ImageUrl = "/images/products/mechanical-keyboard.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new Product
                    {
                        Name = "Premium Italian Leather Weekender Bag",
                        Description = "Handcrafted from full-grain vegetable-tanned leather, heavy-duty brass hardware, reinforced laptop compartment, and adjustable shoulder strap.",
                        Price = 275.00m,
                        AvailableQuantity = 12,
                        CategoryId = fashion.Id,
                        SellerId = seller2.Id,
                        ImageUrl = "/images/products/leather-bag.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-28)
                    },
                    new Product
                    {
                        Name = "Merino Wool Crewneck Sweater",
                        Description = "100% extrafine Australian Merino wool. Ultra-soft, naturally temperature-regulating, breathable, and designed for versatile all-season comfort.",
                        Price = 89.95m,
                        AvailableQuantity = 50,
                        CategoryId = fashion.Id,
                        SellerId = seller2.Id,
                        ImageUrl = "/images/products/merino-sweater.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-18)
                    },
                    new Product
                    {
                        Name = "Barista Pro Espresso Machine",
                        Description = "15-bar Italian pump, integrated precision conical burr grinder, digital PID temperature control, and professional microfoam steam wand.",
                        Price = 599.00m,
                        AvailableQuantity = 8,
                        CategoryId = home.Id,
                        SellerId = seller1.Id,
                        ImageUrl = "/images/products/espresso-machine.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-12)
                    },
                    new Product
                    {
                        Name = "Ergonomic Mesh Executive Chair",
                        Description = "Dynamic lumbar support, 3D adjustable armrests, breathable high-density mesh back, and pneumatic seat height adjustment.",
                        Price = 320.00m,
                        AvailableQuantity = 15,
                        CategoryId = home.Id,
                        SellerId = seller2.Id,
                        ImageUrl = "/images/products/ergonomic-chair.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new Product
                    {
                        Name = "Adjustable Quick-Select Dumbbells Set (5-50 lbs)",
                        Description = "Compact space-saving design with smooth selector dial system, anti-slip textured knurled steel grips, and heavy-duty storage trays.",
                        Price = 299.00m,
                        AvailableQuantity = 20,
                        CategoryId = sports.Id,
                        SellerId = seller2.Id,
                        ImageUrl = "/images/products/adjustable-dumbbells.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
            else if (await context.Products.AnyAsync())
            {
                // Ensure existing database products are updated with their dedicated realistic images
                var existing = await context.Products.ToListAsync();
                foreach (var p in existing)
                {
                    if (p.Name.Contains("Headphones")) p.ImageUrl = "/images/products/wireless-headphones.jpg";
                    else if (p.Name.Contains("Monitor")) p.ImageUrl = "/images/products/4k-monitor.jpg";
                    else if (p.Name.Contains("Keyboard")) p.ImageUrl = "/images/products/mechanical-keyboard.jpg";
                    else if (p.Name.Contains("Leather") || p.Name.Contains("Bag")) p.ImageUrl = "/images/products/leather-bag.jpg";
                    else if (p.Name.Contains("Sweater")) p.ImageUrl = "/images/products/merino-sweater.jpg";
                    else if (p.Name.Contains("Espresso") || p.Name.Contains("Barista")) p.ImageUrl = "/images/products/espresso-machine.jpg";
                    else if (p.Name.Contains("Chair")) p.ImageUrl = "/images/products/ergonomic-chair.jpg";
                    else if (p.Name.Contains("Dumbbells")) p.ImageUrl = "/images/products/adjustable-dumbbells.jpg";
                }
                await context.SaveChangesAsync();
            }

            // 7. Seed Sample Order for Customer 1 (so Customer 1 has verified purchases for reviews!)
            if (!await context.Orders.AnyAsync() && customer1 != null)
            {
                var headphones = await context.Products.FirstOrDefaultAsync(p => p.Name.Contains("Headphones"));
                var sweater = await context.Products.FirstOrDefaultAsync(p => p.Name.Contains("Sweater"));

                if (headphones != null && sweater != null)
                {
                    var order = new Order
                    {
                        CustomerId = customer1.Id,
                        OrderDate = DateTime.UtcNow.AddDays(-14),
                        Status = OrderStatus.Delivered,
                        ShippingAddress = "742 Evergreen Terrace",
                        City = "Springfield",
                        PostalCode = "97477",
                        PhoneNumber = "+1-555-0201",
                        Notes = "Please leave at front door porch.",
                        TotalAmount = headphones.Price + (sweater.Price * 2),
                        Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductId = headphones.Id,
                                SellerId = headphones.SellerId,
                                Quantity = 1,
                                UnitPrice = headphones.Price
                            },
                            new OrderItem
                            {
                                ProductId = sweater.Id,
                                SellerId = sweater.SellerId,
                                Quantity = 2,
                                UnitPrice = sweater.Price
                            }
                        }
                    };

                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    // Seed Verified Review from Customer 1 for Headphones
                    var review1 = new Review
                    {
                        ProductId = headphones.Id,
                        CustomerId = customer1.Id,
                        Rating = 5,
                        Comment = "Phenomenal sound stage and supreme comfort! The noise cancellation completely eliminates background engine noise on flights.",
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    };

                    context.Reviews.Add(review1);
                    await context.SaveChangesAsync();
                }
            }

            // 8. Seed a pending seller request from customer2 for Admin review demonstration
            if (!await context.SellerRequests.AnyAsync() && customer2 != null)
            {
                var sellerReq = new SellerRequest
                {
                    UserId = customer2.Id,
                    BusinessName = "Sophia's Artisan Home Decor",
                    Reason = "We design and craft eco-friendly ceramic pottery, scented soy candles, and minimalist wooden planters. We want to reach national AlaSaree3 buyers.",
                    PhoneNumber = "+1-555-0202",
                    Status = RequestStatus.Pending,
                    RequestedAt = DateTime.UtcNow.AddDays(-2)
                };

                context.SellerRequests.Add(sellerReq);
                await context.SaveChangesAsync();
            }
        }
    }
}
