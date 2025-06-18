using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Room_App.Models
{
    [Table("rooms")]
    public class Room
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("capacity")]
        public int Capacity { get; set; }

        [Required]
        [Column("location")]
        [StringLength(100)]
        public string Location { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("photo_url")]
        [StringLength(255)]
        public string PhotoUrl { get; set; }

        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<string> Facilities { get; set; } = new List<string>();

        // Navigation properties
        public virtual ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}









/*using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Room_App.Models
{
    [Table("rooms")]
    public class Room
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("capacity")]
        public int Capacity { get; set; }

        [Required]
        [Column("location")]
        [StringLength(100)]
        public string Location { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("photo_url")]
        [StringLength(255)]
        public string PhotoUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}*/