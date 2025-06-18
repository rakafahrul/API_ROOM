using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Room_App.Data;
using Room_App.Models;
using Microsoft.AspNetCore.Authorization;

namespace Room_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Facilities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacilityDTO>>> GetFacilities()
        {
            var facilities = await _context.Facilities.ToListAsync();
            return facilities.Select(f => new FacilityDTO
            {
                Id = f.Id,
                Name = f.Name,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        // GET: api/Facilities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FacilityDTO>> GetFacility(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);

            if (facility == null)
            {
                return NotFound();
            }

            return new FacilityDTO
            {
                Id = facility.Id,
                Name = facility.Name,
                CreatedAt = facility.CreatedAt
            };
        }

        // POST: api/Facilities
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FacilityDTO>> CreateFacility(FacilityCreateDTO facilityDto)
        {
            var facility = new Facility
            {
                Name = facilityDto.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFacility),
                new { id = facility.Id },
                new FacilityDTO
                {
                    Id = facility.Id,
                    Name = facility.Name,
                    CreatedAt = facility.CreatedAt
                });
        }

        // PUT: api/Facilities/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFacility(int id, FacilityCreateDTO facilityDto)
        {
            var facility = await _context.Facilities.FindAsync(id);

            if (facility == null)
            {
                return NotFound();
            }

            facility.Name = facilityDto.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FacilityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Facilities/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            // Periksa apakah fasilitas digunakan oleh ruangan
            var roomFacilities = await _context.RoomFacilities
                .Where(rf => rf.FacilityId == id)
                .ToListAsync();

            if (roomFacilities.Any())
            {
                return BadRequest("Cannot delete facility because it is used by one or more rooms");
            }

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FacilityExists(int id)
        {
            return _context.Facilities.Any(e => e.Id == id);
        }
    }
}