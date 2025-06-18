using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Room_App.Models;
using Room_App.Services;

namespace Room_App.Controllers
{
    [ApiController]
    [Route("api/photo_usage")]
    public class PhotoUsageController : ControllerBase
    {
        private readonly IPhotoUsageService _photoService;

        public PhotoUsageController(IPhotoUsageService photoService)
        {
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhotoUsage>>> GetPhotos([FromQuery] int? booking_id)
        {
            try
            {
                if (booking_id.HasValue)
                {
                    var photos = await _photoService.GetPhotosByBookingIdAsync(booking_id.Value);
                    return Ok(photos);
                }
                else
                {
                    var photos = await _photoService.GetAllPhotosAsync();
                    return Ok(photos);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoUsage>> GetPhotoById(int id)
        {
            try
            {
                var photo = await _photoService.GetPhotoByIdAsync(id);
                if (photo == null)
                    return NotFound($"Photo with ID {id} not found");

                return Ok(photo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PhotoUsage>> CreatePhoto(PhotoUsage photo)
        {
            try
            {
                var createdPhoto = await _photoService.CreatePhotoAsync(photo);
                return CreatedAtAction(nameof(GetPhotoById), new { id = createdPhoto.PhotoId }, createdPhoto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            try
            {
                var deleted = await _photoService.DeletePhotoAsync(id);
                if (!deleted)
                    return NotFound($"Photo with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}