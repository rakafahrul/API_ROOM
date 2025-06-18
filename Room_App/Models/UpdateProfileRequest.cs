using System.ComponentModel.DataAnnotations;

namespace Room_App.Models
{
    public class UpdateProfileRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        public string? Photo { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
    }
}