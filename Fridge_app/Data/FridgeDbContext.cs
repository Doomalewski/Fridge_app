using Microsoft.EntityFrameworkCore;

namespace Fridge_app.Data
{
    using Fridge_app.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    public class FridgeDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<StoredProduct> StoredProducts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<ProductWithAmount> ProductWithAmounts { get; set; }
        public DbSet<Diet> Diets { get; set; }
        public DbSet<HumanStats> HumanStats { get; set; }
        public DbSet<WeightEntry> WeightEntries { get; set; }

        public FridgeDbContext(DbContextOptions<FridgeDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
            // === User ===
            modelBuilder.Entity<User>()
                .HasMany(u => u.Fridge)
                .WithOne(sp => sp.User)
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // === StoredProduct ===
            modelBuilder.Entity<StoredProduct>()
                .HasOne(sp => sp.Product)
                .WithMany()
                .HasForeignKey(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            // === Diet ===
            modelBuilder.Entity<Diet>()
                .HasMany(d => d.Meals)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meal>()
                .HasMany(m => m.Tags)
                .WithMany(t => t.Meals)
                .UsingEntity<Dictionary<string, object>>(
                    "MealTag", // Tabela pośrednia
                    j => j
                        .HasOne<Tag>()
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Meal>()
                        .WithMany()
                        .HasForeignKey("MealId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("MealId", "TagId");
                        j.ToTable("MealTags");
                    });

            // === Recipe ===
            modelBuilder.Entity<Recipe>()
                .HasMany(r => r.Products)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            // === ProductWithAmount ===
            modelBuilder.Entity<ProductWithAmount>()
                .HasOne(pwa => pwa.Product)
                .WithMany()
                .HasForeignKey(pwa => pwa.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ProductWithAmount>()
                .HasOne(p => p.Recipe)
                .WithMany(r => r.Products)
                .HasForeignKey(p => p.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            // === HumanStats ===
            modelBuilder.Entity<HumanStats>()
                .HasMany(hs => hs.Weight)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            // === Optional: Configure Value Conversion ===
            modelBuilder.Entity<Product>()
                .Property(p => p.ProductCategory)
                .HasConversion<string>();

            modelBuilder.Entity<Product>()
                .Property(p => p.Unit)
                .HasConversion<string>();

            modelBuilder.Entity<Tag>()
                .Property(t => t.Type)
                .HasConversion<string>();
        }
    }


}
