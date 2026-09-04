using Microsoft.EntityFrameworkCore;
using AlaSaree3.Models;

namespace AlaSaree3.Data
{
    /// <summary>
    /// Seeds the small amount of demo data the AI shopping assistant needs: platform-wide
    /// policies, a couple of example seller-specific policies (that intentionally differ from
    /// the platform defaults and from each other, to prove the assistant keeps them separate),
    /// and some example product sizes. Runs after SeedData.InitializeAsync, so the sellers,
    /// categories and products it references already exist.
    /// </summary>
    public static class ChatbotSeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedPlatformPoliciesAsync(context);
            await SeedSellerPoliciesAsync(context);
            await SeedProductSizesAsync(context);
        }

        private static async Task SeedPlatformPoliciesAsync(ApplicationDbContext context)
        {
            if (await context.PlatformPolicies.AnyAsync())
            {
                return;
            }

            var policies = new List<PlatformPolicy>
            {
                new PlatformPolicy
                {
                    Key = "Return",
                    Title = "General Return & Refund Policy",
                    Content = "Unless a seller states a different policy on their store page, items purchased on AlaSaree3 " +
                        "can be returned within 14 days of delivery for a full refund, as long as they are unused, in their " +
                        "original packaging, and accompanied by the order receipt. Refunds are issued to the original payment " +
                        "method within 5-7 business days after the returned item is received and inspected. Perishable goods, " +
                        "personalized items, and intimate apparel are not eligible for return unless defective."
                },
                new PlatformPolicy
                {
                    Key = "Shipping",
                    Title = "General Shipping Policy",
                    Content = "Standard shipping across the platform typically takes 3-7 business days depending on the seller's " +
                        "location and the buyer's address. Express shipping (1-3 business days) is available on select listings " +
                        "for an additional fee. Orders are usually processed and handed to the courier within 1-2 business days " +
                        "of purchase. Individual sellers may offer faster or slower shipping windows, which take precedence over " +
                        "this general estimate."
                },
                new PlatformPolicy
                {
                    Key = "Cancellation",
                    Title = "General Cancellation Policy",
                    Content = "Orders can be cancelled free of charge as long as they are still in 'Pending' or 'Confirmed' status " +
                        "and have not yet been shipped. Once an order's status changes to 'Shipped', it can no longer be cancelled " +
                        "and must instead be handled through the return process after delivery. To cancel an eligible order, go to " +
                        "'My Orders' and select Cancel, or ask this assistant to cancel it for you."
                },
                new PlatformPolicy
                {
                    Key = "Payment",
                    Title = "General Payment Policy",
                    Content = "AlaSaree3 accepts major credit/debit cards and cash on delivery, depending on the seller and region. " +
                        "Payment is captured at checkout for card payments; cash on delivery orders are paid when the courier " +
                        "hands over the package. All transactions are encrypted and AlaSaree3 never stores full card numbers."
                },
                new PlatformPolicy
                {
                    Key = "Warranty",
                    Title = "General Warranty Policy",
                    Content = "Electronics and appliances sold on AlaSaree3 carry a minimum 12-month manufacturer or seller " +
                        "warranty against defects unless stated otherwise on the product page. Warranty claims are handled by the " +
                        "seller who fulfilled the order; contact them via the order details page, or ask this assistant which " +
                        "seller to reach out to."
                }
            };

            await context.PlatformPolicies.AddRangeAsync(policies);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSellerPoliciesAsync(ApplicationDbContext context)
        {
            if (await context.SellerPolicies.AnyAsync())
            {
                return;
            }

            var seller1 = await context.Users.FirstOrDefaultAsync(u => u.Email == "techstore@alasaree3.com");
            var seller2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "fashionhub@alasaree3.com");

            var policies = new List<SellerPolicy>();

            if (seller1 != null)
            {
                // TechZone Official Store: stricter, electronics-oriented policy that intentionally
                // differs from the platform default, to demonstrate seller-specific overrides.
                policies.Add(new SellerPolicy
                {
                    SellerId = seller1.Id,
                    ReturnPolicy = "TechZone Official Store accepts returns within 30 days of delivery for unopened electronics, " +
                        "and within 7 days for opened items (subject to a 10% restocking fee). Items must include all original " +
                        "accessories and packaging. Defective items can be returned at any time within the warranty period for a " +
                        "free replacement.",
                    ReturnWindowDays = 30,
                    ShippingPolicy = "TechZone ships all in-stock electronics within 24 hours via express courier. Delivery usually " +
                        "takes 2-4 business days nationwide, with free shipping on orders over $150.",
                    WarrantyPolicy = "All electronics sold by TechZone Official Store include a 24-month seller warranty covering " +
                        "manufacturing defects, in addition to any manufacturer warranty. Accidental damage is not covered."
                });
            }

            if (seller2 != null)
            {
                // Urban Trends Boutique: fashion-oriented policy, deliberately different from
                // TechZone's, so the assistant must never mix the two up.
                policies.Add(new SellerPolicy
                {
                    SellerId = seller2.Id,
                    ReturnPolicy = "Urban Trends Boutique accepts returns and exchanges within 14 days of delivery. Garments must be " +
                        "unworn, unwashed, and have all original tags attached. Final-sale items (marked as such on the product page) " +
                        "cannot be returned. Store credit is offered as an alternative to a refund on request.",
                    ReturnWindowDays = 14,
                    ShippingPolicy = "Urban Trends Boutique ships within 2-3 business days of purchase via standard courier, with " +
                        "delivery typically taking 4-6 business days. Express shipping is not currently offered by this seller.",
                    WarrantyPolicy = "Urban Trends Boutique does not offer an extended warranty beyond the platform's standard " +
                        "return window, as apparel items are not covered by manufacturer warranties."
                });
            }

            if (policies.Count > 0)
            {
                await context.SellerPolicies.AddRangeAsync(policies);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedProductSizesAsync(ApplicationDbContext context)
        {
            // Only touch products that don't have size info yet, and only apparel-style items.
            var sweater = await context.Products.FirstOrDefaultAsync(p => p.Name.Contains("Merino Wool"));
            if (sweater != null && string.IsNullOrEmpty(sweater.AvailableSizes))
            {
                sweater.AvailableSizes = "S,M,L,XL";
            }

            await context.SaveChangesAsync();
        }
    }
}
