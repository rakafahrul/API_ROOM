using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Room_App.DTOs
{
    public class CreateBookingRequest
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public string StartTime { get; set; }

        [Required]
        public string EndTime { get; set; }

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public DateTime? CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }

        [StringLength(100)]
        public string LocationGps { get; set; } = "";

        public bool IsPresent { get; set; } = false;

        [StringLength(255)]
        public string RoomPhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<PhotoUsageRequest> Photos { get; set; } = new List<PhotoUsageRequest>();
    }

    public class PhotoUsageRequest
    {
        public string PhotoUrl { get; set; }
    }
}