using Microsoft.EntityFrameworkCore;
using RegistrationService.Data.Entities;

namespace RegistrationService.Data
{
    //1. Make me a class called RegistrationDbContext that is an instance of DbContext
    //2. We do this in order to have a copy of the instance and add new features into it 
    // what new features are we adding and how?
    public class RegistrationDbContext : DbContext //DbContext is an Entity Framework's base class used to handle database operations
    {
        public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options)
            : base(options) { }
        
        //1.Within out tables like 'RegisteredUsers' and 'registeredTeams', I want you to 
        //know each table is a RegisteredUser/RegisteredTeams object. These objecs are
        //classes defining what each column/property has 
        public DbSet<RegisteredUser> RegisteredUsers { get; set; }
        public DbSet<RegisteredTeams> RegisteredTeams { get; set; }  

        //1. Summary: When the app starts, please set up the mapping/blueprint between my C# classes
        // and my database tables. You will tell Entity Framework which C# class connects to which table,
        //what my primary keys are, and what the rules/contraints are 
        //2. protected: make this method private to this class, override: for dbContext, I want you to change 
        //its setting to what I want, void: This method will not return any value, OnModelingCreating: Entity framework intiallzinh this 
        // midelBuilder: a congiuration tool from the framework what does what comment 1 is 
        // Data/RegistrationDbContext.cs
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegisteredUser>(entity => 
            {
                entity.ToTable("RegisteredUsers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<RegisteredTeams>(entity => 
            {
                entity.ToTable("Teams");
                entity.HasKey(e => e.TeamId);
                entity.Property(e => e.TeamId).ValueGeneratedOnAdd();
                
                entity.HasMany(t => t.Students)
                    .WithOne()
                    .HasForeignKey(s => s.TeamId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}