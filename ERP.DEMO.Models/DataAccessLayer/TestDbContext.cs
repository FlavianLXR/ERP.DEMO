using ERP.DEMO.Models.TestDb;
using Microsoft.EntityFrameworkCore;

namespace ERP.DEMO.Models.DataAccessLayer
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
       : base(options)
        {
        }
        #region DbSet properties
        /// <summary>
        /// Obtient ou définit la collection des entités "User" pouvant être interrogées à partir de la base de données.
        /// </summary>
        public DbSet<TestDb.User> Users { get; set; }

        /// <summary>
        /// Obtient ou définit la collection des entités "Order" pouvant être interrogées à partir de la base de données.
        /// </summary>
        public DbSet<TestDb.Order> Orders { get; set; }

        /// <summary>
        /// Obtient ou définit la collection des entités "OrderLine" pouvant être interrogées à partir de la base de données.
        /// </summary>
        public DbSet<TestDb.OrderLine> OrderLines { get; set; }

        /// <summary>
        /// Obtient ou définit la collection des entités "Product" pouvant être interrogées à partir de la base de données.
        /// </summary>
        public DbSet<TestDb.Product> Products { get; set; }

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<int>(); // Force le stockage en int
        }
    }
}
