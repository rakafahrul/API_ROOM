using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Room_App.Models;
using Room_App.Models.DTOs;
using Room_App.Services;

namespace Room_App.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookings()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                var bookingDtos = bookings.Select(b => BookingToDTO(b)).ToList();
                return Ok(bookingDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/bookings/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBookingById(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                    return NotFound($"Booking with ID {id} not found");

                return Ok(BookingToDTO(booking));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<BookingDTO>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var booking = new Booking
                {
                    RoomId = request.RoomId,
                    UserId = request.UserId,
                    BookingDate = request.BookingDate,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Purpose = request.Purpose,
                    Status = request.Status,
                    CheckinTime = request.CheckinTime,
                    CheckoutTime = request.CheckoutTime,
                    LocationGps = request.LocationGps ?? "",
                    IsPresent = request.IsPresent,
                    RoomPhotoUrl = request.RoomPhotoUrl,
                    CreatedAt = request.CreatedAt,
                    Photos = request.Photos?.ConvertAll(p => new PhotoUsage
                    {
                        PhotoUrl = p.PhotoUrl
                    }) ?? new List<PhotoUsage>()
                };

                var createdBooking = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, BookingToDTO(createdBooking));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/bookings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingDTO bookingDto)
        {
            if (id != bookingDto.Id)
                return BadRequest("Booking ID mismatch");

            try
            {
                // Convert BookingDTO to Booking for updating
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                    return NotFound($"Booking with ID {id} not found");

                // Map fields from DTO to entity
                booking.RoomId = bookingDto.RoomId;
                booking.UserId = bookingDto.UserId;
                booking.BookingDate = bookingDto.BookingDate;
                booking.StartTime = bookingDto.StartTime;
                booking.EndTime = bookingDto.EndTime;
                booking.Purpose = bookingDto.Purpose;
                booking.Status = bookingDto.Status;
                booking.CheckinTime = bookingDto.CheckinTime;
                booking.CheckoutTime = bookingDto.CheckoutTime;
                booking.LocationGps = bookingDto.LocationGps;
                booking.IsPresent = bookingDto.IsPresent;
                booking.RoomPhotoUrl = bookingDto.RoomPhotoUrl;
                booking.CreatedAt = bookingDto.CreatedAt;
                // Photos update: Optional, for now ignore (usually update via /checkout or /photo endpoint)

                var updated = await _bookingService.UpdateBookingAsync(booking);
                if (!updated)
                    return NotFound($"Booking with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/bookings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var deleted = await _bookingService.DeleteBookingAsync(id);
                if (!deleted)
                    return NotFound($"Booking with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveBooking(int id, [FromBody] NoteRequest request)
        {
            try
            {
                var approved = await _bookingService.ApproveBookingAsync(id, request.Note);
                if (!approved)
                    return NotFound($"Booking with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectBooking(int id, [FromBody] NoteRequest request)
        {
            try
            {
                var rejected = await _bookingService.RejectBookingAsync(id, request.Note);
                if (!rejected)
                    return NotFound($"Booking with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/checkin")]
        public async Task<IActionResult> Checkin(int id, [FromBody] CheckinRequest request)
        {
            try
            {
                var checkedIn = await _bookingService.CheckinAsync(id, request.LocationGps);
                if (!checkedIn)
                    return NotFound($"Booking with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/checkout")]
        public async Task<IActionResult> Checkout(int id, [FromBody] CheckoutRequest request)
        {
            try
            {
                var checkedOut = await _bookingService.CheckoutAsync(id, request.PhotoUrl);
                if (!checkedOut)
                    return NotFound($"Booking with ID {id} not found or not checked in");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // --- Helper: Map Booking to BookingDTO ---
        private BookingDTO BookingToDTO(Booking b)
        {
            return new BookingDTO
            {
                Id = b.Id,
                RoomId = b.RoomId,
                RoomName = b.Room?.Name ?? "",
                UserId = b.UserId,
                UserName = b.User?.Name ?? "", // Perbaiki: gunakan UserName, bukan Username
                BookingDate = b.BookingDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Purpose = b.Purpose,
                Status = b.Status,
                CheckinTime = b.CheckinTime,
                CheckoutTime = b.CheckoutTime,
                LocationGps = b.LocationGps,
                IsPresent = b.IsPresent,
                RoomPhotoUrl = b.RoomPhotoUrl,
                CreatedAt = b.CreatedAt,
                PhotoUrls = b.Photos?.Select(p => p.PhotoUrl).ToList() ?? new List<string>()
            };
        }
    }

    // ----- Request DTOs -----
    public class NoteRequest
    {
        public string Note { get; set; }
    }

    public class CheckinRequest
    {
        public string LocationGps { get; set; }
    }

    public class CheckoutRequest
    {
        public string PhotoUrl { get; set; }
    }

    // Request for POST (create)
    public class CreateBookingRequest
    {
        public int RoomId { get; set; }
        public int UserId { get; set; }
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
        public List<PhotoRequest> Photos { get; set; }
    }

    public class PhotoRequest
    {
        public string PhotoUrl { get; set; }
    }
}













/*using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Room_App.Models;
using Room_App.Services;
using Room_App.Models;
using Room_App.Services;
using Room_App.DTOs;

namespace Room_App.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBookingById(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                    return NotFound($"Booking with ID {id} not found");

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Mapping DTO ke Entity
                var booking = new Booking
                {
                    RoomId = request.RoomId,
                    UserId = request.UserId,
                    BookingDate = request.BookingDate,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Purpose = request.Purpose,
                    Status = request.Status,
                    CheckinTime = request.CheckinTime,
                    CheckoutTime = request.CheckoutTime,
                    LocationGps = request.LocationGps,
                    IsPresent = request.IsPresent,
                    RoomPhotoUrl = request.RoomPhotoUrl,
                    CreatedAt = request.CreatedAt,
                    Photos = request.Photos?.ConvertAll(p => new PhotoUsage
                    {
                        PhotoUrl = p.PhotoUrl
                    }) ?? new List<PhotoUsage>()
                };

                var createdBooking = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        *//*[HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
        {
            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }*//*

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, Booking booking)
        {
            if (id != booking.Id)
                return BadRequest("Booking ID mismatch");

            try
            {
                var updated = await _bookingService.UpdateBookingAsync(booking);
                if (!updated)
                    return NotFound($"Booking with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var deleted = await _bookingService.DeleteBookingAsync(id);
                if (!deleted)
                    return NotFound($"Booking with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveBooking(int id, [FromBody] NoteRequest request)
        {
            try
            {
                var approved = await _bookingService.ApproveBookingAsync(id, request.Note);
                if (!approved)
                    return NotFound($"Booking with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectBooking(int id, [FromBody] NoteRequest request)
        {
            try
            {
                var rejected = await _bookingService.RejectBookingAsync(id, request.Note);
                if (!rejected)
                    return NotFound($"Booking with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/checkin")]
        public async Task<IActionResult> Checkin(int id, [FromBody] CheckinRequest request)
        {
            try
            {
                var checkedIn = await _bookingService.CheckinAsync(id, request.LocationGps);
                if (!checkedIn)
                    return NotFound($"Booking with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/checkout")]
        public async Task<IActionResult> Checkout(int id, [FromBody] CheckoutRequest request)
        {
            try
            {
                var checkedOut = await _bookingService.CheckoutAsync(id, request.PhotoUrl);
                if (!checkedOut)
                    return NotFound($"Booking with ID {id} not found or not checked in");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class NoteRequest
    {
        public string Note { get; set; }
    }

    public class CheckinRequest
    {
        public string LocationGps { get; set; }
    }

    public class CheckoutRequest
    {
        public string PhotoUrl { get; set; }
    }
}*/