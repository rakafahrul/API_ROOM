using Microsoft.EntityFrameworkCore;
using Room_App.Models;
using Room_App.Models;

namespace Room_App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PhotoUsage> PhotoUsages { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<RoomFacility> RoomFacilities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Room>().ToTable("rooms");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<PhotoUsage>().ToTable("photo_usages");
            modelBuilder.Entity<Facility>().ToTable("facilities");
            modelBuilder.Entity<RoomFacility>().ToTable("room_facilities");


            modelBuilder.Entity<RoomFacility>()
                .HasOne(rf => rf.Room)
                .WithMany(r => r.RoomFacilities)
                .HasForeignKey(rf => rf.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomFacility>()
                .HasOne(rf => rf.Facility)
                .WithMany(f => f.RoomFacilities)
                .HasForeignKey(rf => rf.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<RoomFacility>()
                .HasIndex(rf => new { rf.RoomId, rf.FacilityId })
                .IsUnique();
        }
    }
}