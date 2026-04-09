using Bogus;
using ERP.DEMO.Models.DataAccessLayer;
using ERP.DEMO.Models.TestDb;
using static ERP.DEMO.Models.TestDb.Order;

namespace ERP.DEMO.Components.MVVM
{
    public class DbSeederService
    {
        public static void Seed(TestDbContext context)
        {
            context.OrderLines.RemoveRange(context.OrderLines);
            context.Orders.RemoveRange(context.Orders);
            context.Products.RemoveRange(context.Products);
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            if (!context.Users.Any())
            {

                // User fixe connu avant le faker
                context.Users.Add(new User
                {
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin"),
                    LastName = "Admin",
                    FirstName = "Super",
                    Email = "admin@demo.fr",
                    IsActive = true,
                    CreationDate = DateTime.Now,
                    ModificationDate = DateTime.Now,
                    Role = 1
                });
                context.SaveChanges();

                // ── 1. USERS ──────────────────────────────────────────────
                var userFaker = new Faker<User>("fr")
                    .RuleFor(u => u.Username, f => f.Internet.UserName())
                    .RuleFor(u => u.Password, f => BCrypt.Net.BCrypt.HashPassword("test"))
                    .RuleFor(u => u.LastName, f => f.Name.LastName())
                    .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                    .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                    .RuleFor(u => u.IsActive, f => f.Random.Bool(0.9f)) // 90% actifs
                    .RuleFor(u => u.LastLoginDate, f => f.Date.Recent(30))
                    .RuleFor(u => u.CreationDate, f => f.Date.Past(2))
                    .RuleFor(u => u.ModificationDate, f => f.Date.Recent(60))
                    .RuleFor(u => u.Role, f => f.PickRandom<short>(new short[] { 1, 2, 3 }));
                var users = userFaker.Generate(50);
                context.Users.AddRange(users);
                context.SaveChanges();

            }

            var userIds = context.Users.Select(u => u.Id).ToList();

            if (!context.Products.Any())
            {

                // ── 2. PRODUCTS ───────────────────────────────────────────
                var categories = new[] { "ELEC", "MECA", "CONS", "INFO", "LOGI" };
                var productFaker = new Faker<Product>("fr")
                    .RuleFor(p => p.Label, f => f.Commerce.ProductName())
                    .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
                    .RuleFor(p => p.Price, f => f.Finance.Amount(5, 5000))
                    .RuleFor(p => p.Quantity, f => f.Random.Int(0, 500))
                    .RuleFor(p => p.IsActive, f => f.Random.Bool(0.85f))
                    .RuleFor(p => p.Length, f => (double?)f.Random.Double(1, 200))
                    .RuleFor(p => p.Weight, f => (double?)f.Random.Double(0.1, 50))
                    .RuleFor(p => p.Height, f => (double?)f.Random.Double(1, 100))
                    .RuleFor(p => p.Width, f => (double?)f.Random.Double(1, 100))
                    .RuleFor(p => p.CreationDate, f => f.Date.Past(3))
                    .RuleFor(p => p.CreatedBy, f => f.PickRandom(userIds))
                    .RuleFor(p => p.ModificationDate, f => f.Date.Recent(90))
                    .RuleFor(p => p.ModifiedBy, f => f.PickRandom(userIds));

                var products = productFaker.Generate(200);

                context.Products.AddRange(products);
                context.SaveChanges();

            }

            var productIds = context.Products.Select(p => p.Id).ToList();
            var statuses = Enum.GetValues<OrderStatusList>().ToList();

            if (!context.Orders.Any())
            {

                // ── 3. ORDERS + ORDERLINES ────────────────────────────────
                var orderFaker = new Faker<Order>("fr")
            .RuleFor(o => o.Label, f => $"CMD-{f.Date.Recent(365):yyyyMM}-{f.Random.AlphaNumeric(4).ToUpper()}")
            .RuleFor(o => o.UserId, f => f.PickRandom(userIds))
            .RuleFor(o => o.Status, f => f.PickRandom(statuses))
            .RuleFor(o => o.IsPriority, f => f.Random.Bool(0.2f)) // 20% prioritaires
            .RuleFor(o => o.DeliveryNotes, f => f.Random.Bool(0.4f) ? f.Lorem.Sentence() : null)
            .RuleFor(o => o.PlannedDeliveryDate, f => f.Random.Bool(0.7f) ? f.Date.Future(1) : null)
            .RuleFor(o => o.CreationDate, f => f.Date.Past(2))
            .RuleFor(o => o.CreatedBy, f => f.PickRandom(userIds))
            .RuleFor(o => o.ModificationDate, f => f.Date.Recent(60))
            .RuleFor(o => o.ModifiedBy, f => f.Random.Bool(0.6f) ? f.PickRandom(userIds) : null);

                var orders = orderFaker.Generate(300);
                context.Orders.AddRange(orders);
                context.SaveChanges();

            }

            var orderIds = context.Orders.Select(o => o.Id).ToList();

            if (!context.OrderLines.Any())
            {

                // ── 4. ORDERLINES ─────────────────────────────────────────
                var orderLines = new List<OrderLine>();
                var fakerBase = new Faker("fr");

                foreach (var orderId in orderIds)
                {
                    // Chaque commande a entre 1 et 8 lignes
                    var lineCount = fakerBase.Random.Int(1, 8);
                    var pickedProducts = fakerBase.PickRandom(productIds, lineCount).Distinct().ToList();

                    foreach (var productId in pickedProducts)
                    {
                        orderLines.Add(new OrderLine
                        {
                            OrderId = orderId,
                            ProductId = productId,
                            Quantity = fakerBase.Random.Int(1, 50)
                        });
                    }
                }

                context.OrderLines.AddRange(orderLines);
                context.SaveChanges();
            }
        }
    }
}
