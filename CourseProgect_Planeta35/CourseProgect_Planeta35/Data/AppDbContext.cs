using Microsoft.EntityFrameworkCore;
using CourseProgect_Planeta35.Models;

namespace CourseProgect_Planeta35.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AssetCategory> AssetCategories { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Подключение к твоей SQL Server
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=Planet35;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
);
        }
    }
}
