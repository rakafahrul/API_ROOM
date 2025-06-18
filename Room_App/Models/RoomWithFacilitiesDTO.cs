namespace Room_App.Models
{
    public class RoomWithFacilitiesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string PhotoUrl { get; set; }
        public List<string> Facilities { get; set; }
    }
}
