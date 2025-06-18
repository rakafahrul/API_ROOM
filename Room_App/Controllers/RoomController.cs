using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Room_App.Models;
using Room_App.Services;
using Room_App.Models;

namespace Room_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        [Route("/api/meeting_rooms")]
        public async Task<ActionResult<IEnumerable<Room>>> GetAllRooms()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsWithFacilitiesAsync();
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoomById(int id)
        {
            try
            {
                var room = await _roomService.GetRoomByIdAsync(id);
                if (room == null)
                    return NotFound($"Room with ID {id} not found");

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("/api/rooms")]
        public async Task<ActionResult<Room>> CreateRoom([FromForm] RoomCreateDto dto, IFormFile photo)
        {
            try
            {
                // Simpan gambar ke folder wwwroot/uploads
                string photoUrl = null;
                if (photo != null && photo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    // Simpan URL gambar relatif
                    photoUrl = $"/uploads/{fileName}";
                }

                // Buat objek Room
                var room = new Room
                {
                    Name = dto.Name,
                    Capacity = dto.Capacity,
                    Location = dto.Location,
                    Description = dto.Description,
                    PhotoUrl = photoUrl,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    CreatedAt = DateTime.Now,
                    Facilities = dto.Facilities ?? new List<string>()
                };

                var createdRoom = await _roomService.CreateRoomAsync(room);
                return CreatedAtAction(nameof(GetRoomById), new { id = createdRoom.Id }, createdRoom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /*
                [HttpPost]
                [Route("/api/rooms")]
                public async Task<ActionResult<Room>> CreateRoom(Room room)
                {
                    try
                    {
                        var createdRoom = await _roomService.CreateRoomAsync(room);
                        return CreatedAtAction(nameof(GetRoomById), new { id = createdRoom.Id }, createdRoom);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }*/

        [HttpPut]
        [Route("/api/rooms/{id}")]
        public async Task<IActionResult> UpdateRoom(int id, Room room)
        {
            if (id != room.Id)
                return BadRequest("Room ID mismatch");

            try
            {
                var updated = await _roomService.UpdateRoomAsync(room);
                if (!updated)
                    return NotFound($"Room with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpDelete]
        [Route("/api/rooms/{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var deleted = await _roomService.DeleteRoomAsync(id);
                if (!deleted)
                    return NotFound($"Room with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}