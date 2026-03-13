using Backend_Clothes_API.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace Backend_Clothes_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext ( DbContextOptions<ApplicationDbContext> options ) : base ( options )
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                entity.HasIndex(e => e.UserName)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cart configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Token)
                    .IsUnique()
                    .HasDatabaseName("IX_RefreshTokens_Token");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_RefreshTokens_UserId");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed Data
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var categoryId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var fixedDate = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                UserName = "admin",
                Email = "admin@clothes.com",
                PasswordHash = "$2a$11$mC5zJ9tYnS/jE6xS9A/f7u3p6oV0Yh3gN7B8y7V.N6z8J1p7rV2i2",
                Role = "Admin",
                IsActive = true,
                CreatedAt = fixedDate,
                UpdatedAt = fixedDate
            });

            modelBuilder.Entity<Category>().HasData(new Category
            {
                Id = categoryId,
                Name = "Default Category",
                Description = "Auto-generated category",
                CreatedAt = fixedDate,
                UpdatedAt = fixedDate
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => (e.Entity is User || e.Entity is Category || e.Entity is Product || e.Entity is Cart || e.Entity is CartItem) 
                            && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;
                if (entry.Entity is User user) user.UpdatedAt = now;
                else if (entry.Entity is Category category) category.UpdatedAt = now;
                else if (entry.Entity is Product product) product.UpdatedAt = now;
                else if (entry.Entity is Cart cart) cart.UpdatedAt = now;
                else if (entry.Entity is CartItem cartItem) cartItem.UpdatedAt = now;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }

}
