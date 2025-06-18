using System.Collections.Generic;
using System.Threading.Tasks;
using Room_App.Models;
using Room_App.Models;

namespace Room_App.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room> GetRoomByIdAsync(int id);
        Task<Room> CreateRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(Room room);
        Task<bool> DeleteRoomAsync(int id);
        Task<List<RoomWithFacilitiesDTO>> GetAllRoomsWithFacilitiesAsync();
    }
}