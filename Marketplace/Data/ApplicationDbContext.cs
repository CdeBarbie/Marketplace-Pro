using Microsoft.EntityFrameworkCore;
using MarketPlace.Models;
using BCrypt.Net;

namespace MarketPlace.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
            });

            // Customer entity configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithOne(u => u.Customer)
                      .HasForeignKey<Customer>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Vendor entity configuration
            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.HasOne(v => v.User)
                      .WithOne(u => u.Vendor)
                      .HasForeignKey<Vendor>(v => v.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ApprovalStatus).HasConversion<string>();
            });

            // Product entity configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.Vendor)
                      .WithMany(v => v.Products)
                      .HasForeignKey(p => p.VendorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.Price);
            });

            // Category entity configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(c => c.ParentCategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Order entity configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasIndex(o => o.OrderDate);
            });

            // OrderItem entity configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint for order-product combination
                entity.HasIndex(oi => new { oi.OrderId, oi.ProductId }).IsUnique();
            });

            // Payment entity configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.Order)
                      .WithOne(o => o.Payment)
                      .HasForeignKey<Payment>(p => p.OrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.PaymentMethod).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasIndex(p => p.TransactionId).IsUnique();
            });

            // Review entity configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Customer)
                      .WithMany(c => c.Reviews)
                      .HasForeignKey(r => r.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint to prevent multiple reviews from same customer for same product
                entity.HasIndex(r => new { r.CustomerId, r.ProductId }).IsUnique();
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", Description = "Electronic devices and gadgets" },
                new Category { CategoryId = 2, Name = "Clothing", Description = "Apparel and fashion items" },
                new Category { CategoryId = 3, Name = "Books", Description = "Books and educational materials" },
                new Category { CategoryId = 4, Name = "Home & Garden", Description = "Home improvement and gardening supplies" },
                new Category { CategoryId = 5, Name = "Sports", Description = "Sports equipment and accessories" }
            );

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@marketplace.com",
                    Role = UserRole.Admin,
                    Status = UserStatus.Active
                }
            );

            // Seed Sample Vendor User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 2,
                    Username = "vendor1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("vendor123"),
                    Email = "vendor1@marketplace.com",
                    Role = UserRole.Vendor,
                    Status = UserStatus.Active
                }
            );

            // Seed Sample Customer User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 3,
                    Username = "customer1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                    Email = "customer1@marketplace.com",
                    Role = UserRole.Customer,
                    Status = UserStatus.Active
                }
            );

            // Seed Vendor record
            modelBuilder.Entity<Vendor>().HasData(
                new Vendor
                {
                    VendorId = 1,
                    UserId = 2,
                    BusinessName = "TechStore Pro",
                    BusinessDescription = "Premium electronics and gadgets retailer",
                    ApprovalStatus = ApprovalStatus.Approved
                }
            );

            // Seed Customer record
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    CustomerId = 1,
                    UserId = 3,
                    Address = "123 Main Street, City, State 12345",
                    PhoneNumber = "+1-555-0123"
                }
            );

            // Seed Sample Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    VendorId = 1,
                    Name = "Wireless Headphones",
                    Description = "High-quality wireless headphones with noise cancellation technology. Perfect for music lovers and professionals.",
                    Price = 199.99m,
                    StockQuantity = 50,
                    CategoryId = 1,
                    Status = ProductStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    ProductId = 2,
                    VendorId = 1,
                    Name = "Bluetooth Speaker",
                    Description = "Portable Bluetooth speaker with excellent sound quality and long battery life.",
                    Price = 89.99m,
                    StockQuantity = 30,
                    CategoryId = 1,
                    Status = ProductStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    ProductId = 3,
                    VendorId = 1,
                    Name = "Programming Guide",
                    Description = "Complete guide to modern programming practices and software development.",
                    Price = 49.99m,
                    StockQuantity = 75,
                    CategoryId = 3,
                    Status = ProductStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is User && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                ((User)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
