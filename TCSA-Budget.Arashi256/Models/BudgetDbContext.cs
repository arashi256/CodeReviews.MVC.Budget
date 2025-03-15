using Microsoft.EntityFrameworkCore;

namespace TCSA_Budget.Arashi256.Models
{
    public class BudgetDbContext : DbContext
    {
        public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options) {}
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Utilities" },
                new Category { Id = 2, Name = "Food" },
                new Category { Id = 3, Name = "Housing" },
                new Category { Id = 4, Name = "Transportation" }
            );
            // Configure one-to-many relationship with cascade delete.
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Transactions)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique(); // Enforce uniqueness for category name at the database level.
            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(20); // Ensures database enforces string length.
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2); // Same as decimal(18,2) - gets rid of warning in EF Core console.
            modelBuilder.Entity<Transaction>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Transaction_Amount", "Amount > 0")); // Enforces positive numbers for Amount.
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Description)
                .HasMaxLength(255) // Limit description length.
                .IsRequired(); // Ensures NOT NULL.
        }
    }
}