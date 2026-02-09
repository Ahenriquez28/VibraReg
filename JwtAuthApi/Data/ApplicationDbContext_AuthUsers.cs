using Microsoft.EntityFrameworkCore;
using JwtAuthApi.Models;

namespace JwtAuthApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> AuthUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity for authUsers table
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("authUsers");
            
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            
            entity.Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();
            
            entity.Property(e => e.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(50);
            
            entity.Property(e => e.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(50);
            
            entity.Property(e => e.Roles)
                .HasColumnName("roles")
                .HasDefaultValue("User");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.LastLoginAt)
                .HasColumnName("last_login_at");
            
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);
        });
    }
}