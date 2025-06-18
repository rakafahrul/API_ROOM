using Room_App.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace Room_App.Models
{
    [Table("photo_usages")]
    public class PhotoUsage
    {
        [Key]
        [Column("photo_id")]
        public int PhotoId { get; set; }

        [JsonIgnore]
        [ForeignKey("booking_id")]
        public int BookingId { get; set; }

        [Column("photo_url")]
        public string PhotoUrl { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}