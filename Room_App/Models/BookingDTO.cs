namespace Room_App.Models.DTOs
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public DateTime? CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public string LocationGps { get; set; }
        public bool IsPresent { get; set; }
        public string RoomPhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }
}