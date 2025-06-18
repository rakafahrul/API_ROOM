namespace Room_App.Models
{
    public class RoomCreateDto
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public IFormFile? Photo { get; set; }
        public List<string> Facilities { get; set; }

    }
}
