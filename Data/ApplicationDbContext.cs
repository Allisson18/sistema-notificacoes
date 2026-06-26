using Microsoft.EntityFrameworkCore;
using NotificationHub00.Entities;

namespace NotificationHub00.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }


        // Construtor exigido pelo EF Core
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Configurações explícitas de boas práticas para a tabela
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Target).IsRequired().HasMaxLength(150);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
                entity.Property(n => n.Channel).IsRequired().HasMaxLength(20);
                entity.Property(n => n.CreatedAt).IsRequired();
            });
        }
    }
}
