namespace Room_App.Models
{
    public class RoomUpdateDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Capacity { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public string PhotoUrl { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Kalau mau handle fasilitas juga, gunakan list string atau ID:
        public List<string> Facilities { get; set; }
    }
}
