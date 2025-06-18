using System;
using System.ComponentModel.DataAnnotations;

namespace Room_App.Models
{
    public class FacilityCreateDTO
    {
        [Required]
        public string Name { get; set; }
    }

    // DTO untuk mengirim data ke client
    public class FacilityDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


