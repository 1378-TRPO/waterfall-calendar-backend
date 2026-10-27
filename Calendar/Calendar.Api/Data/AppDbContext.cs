using Calendar.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
        public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();
                e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();

                // Хэши фиксированы, чтобы миграция не менялась при каждой генерации.
                // Пароли: alice123, bob123, carol123.
                var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                e.HasData(
                    new User
                    {
                        Id = 1,
                        Email = "alice@local",
                        PasswordHash = "$2a$11$UNY1lYPk64KQGKhqdQB4cextM0oiGzIWZ7mSSqkkbTiILttCOF5Uq",
                        DisplayName = "Alice",
                        CreatedAt = seededAt
                    },
                    new User
                    {
                        Id = 2,
                        Email = "bob@local",
                        PasswordHash = "$2a$11$0KSGXg6vbVp.Ygy82xKjDeAqwHkWp9TcUyu13RG6XUo/Yqw7mvLWO",
                        DisplayName = "Bob",
                        CreatedAt = seededAt
                    },
                    new User
                    {
                        Id = 3,
                        Email = "carol@local",
                        PasswordHash = "$2a$11$UTI/EJUeQvFSRaMoZSGiGuXW2u6GwC7B7f4edRRkEROjWz7U4lSIK",
                        DisplayName = "Carol",
                        CreatedAt = seededAt
                    });
            });

            modelBuilder.Entity<CalendarEvent>(e =>
            {
                e.ToTable(t => t.HasCheckConstraint("CK_CalendarEvents_EndAt_After_StartAt", "\"EndAt\" > \"StartAt\""));
                e.Property(x => x.Type).HasLowercaseConversion().HasMaxLength(16);
                e.Property(x => x.Title).HasMaxLength(200).IsRequired();
                e.Property(x => x.MeetingUrl).HasMaxLength(500);
                e.HasOne(x => x.Organizer)
                    .WithMany(u => u.OrganizedEvents)
                    .HasForeignKey(x => x.OrganizerId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.OrganizerId, x.StartAt });
                e.HasIndex(x => x.StartAt);
            });

            modelBuilder.Entity<EventParticipant>(e =>
            {
                e.HasKey(x => new { x.EventId, x.UserId });
                e.Property(x => x.RsvpStatus).HasLowercaseConversion().HasMaxLength(16);
                e.HasOne(x => x.Event)
                    .WithMany(ev => ev.Participants)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.User)
                    .WithMany(u => u.Participations)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => x.UserId);
            });

            modelBuilder.Entity<Notification>(e =>
            {
                e.Property(x => x.Type).HasLowercaseConversion().HasMaxLength(16);
                e.Property(x => x.Message).HasMaxLength(300).IsRequired();
                e.HasOne(x => x.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Event)
                    .WithMany()
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.UserId, x.IsRead });
            });
        }
    }
}
