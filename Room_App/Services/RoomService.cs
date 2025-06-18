using Microsoft.EntityFrameworkCore;
using Room_App.Data;
using Room_App.Models;
using Room_App.Services;
using Room_App.Data;
using Room_App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Room_App.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms.ToListAsync();
        }

        public async Task<Room> GetRoomByIdAsync(int id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<bool> UpdateRoomAsync(Room room)
        {
            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(room.Id))
                    return false;
                throw;
            }
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<RoomWithFacilitiesDTO>> GetAllRoomsWithFacilitiesAsync()
        {
            var rooms = await _context.Rooms
                .Include(r => r.RoomFacilities)
                .ThenInclude(rf => rf.Facility)
                .ToListAsync();

            return rooms.Select(r => new RoomWithFacilitiesDTO
            {
                Id = r.Id,
                Name = r.Name,
                Capacity = r.Capacity,
                Location = r.Location,
                Description = r.Description,
                PhotoUrl = r.PhotoUrl,
                Facilities = r.RoomFacilities.Select(rf => rf.Facility.Name).ToList()
            }).ToList();
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}