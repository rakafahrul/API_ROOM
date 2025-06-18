using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Room_App.Models
{
    [Table("bookings")]
    public class Booking
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("room_id")]
        [Required]
        public int RoomId { get; set; }

        [ForeignKey("user_id")]
        [Required]
        public int UserId { get; set; }

        [Required]
        [Column("booking_date")]
        public DateTime BookingDate { get; set; }

        [Required]
        [Column("start_time")]
        public string StartTime { get; set; }

        [Required]
        [Column("end_time")]
        public string EndTime { get; set; }

        [Required]
        [Column("purpose")]
        [StringLength(500)]
        public string Purpose { get; set; }

        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; }

        [Column("checkin_time")]
        public DateTime? CheckinTime { get; set; }

        [Column("checkout_time")]
        public DateTime? CheckoutTime { get; set; }

        [Column("location_gps")]
        [StringLength(100)]
        public string? LocationGps { get; set; } = "";

        [Column("is_present")]
        public bool IsPresent { get; set; } = false;

        [Column("room_photo_url")]
        [StringLength(255)]
        public string? RoomPhotoUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [JsonIgnore]
        public virtual Room Room { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        public virtual ICollection<PhotoUsage> Photos { get; set; } = new List<PhotoUsage>();
    }

    public class CheckInRequest
    {
        [Required]
        public string LocationGps { get; set; }

        [Required]
        public string RoomPhotoUrl { get; set; }
    }
}