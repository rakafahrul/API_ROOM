using Room_App.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace Room_App.Models
{
    [Table("room_facilities")]
    public class RoomFacility
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [JsonIgnore]
        [ForeignKey("room_id")]
        public int RoomId { get; set; }

        [JsonIgnore]
        [ForeignKey("facility_id")]
        public int FacilityId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [ForeignKey("FacilityId")]
        public virtual Facility Facility { get; set; }
    }
}