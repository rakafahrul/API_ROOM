using Microsoft.EntityFrameworkCore;
using Room_App.Data;
using Room_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Room_App.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.Photos)
                .ToListAsync();
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.Photos)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Photos)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByRoomIdAsync(int roomId)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Photos)
                .Where(b => b.RoomId == roomId)
                .ToListAsync();
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            booking.Status = "pending";
            booking.CreatedAt = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.Id))
                    return false;
                throw;
            }
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApproveBookingAsync(int id, string note)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return false;

            booking.Status = "approved";
            // Optionally: booking.ApproveNote = note;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectBookingAsync(int id, string note)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return false;

            booking.Status = "rejected";
            // Optionally: booking.RejectNote = note;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckinAsync(int id, string locationGps)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return false;

            booking.CheckinTime = DateTime.UtcNow;
            booking.LocationGps = locationGps;
            booking.IsPresent = true;
            booking.Status = "in_use";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckoutAsync(int id, string photoUrl)
        {
            var booking = await _context.Bookings
                .Include(b => b.Photos)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null || !booking.IsPresent)
                return false;

            booking.CheckoutTime = DateTime.UtcNow;
            booking.Status = "done";

            // Tambahkan foto ke table PhotoUsage
            if (booking.Photos == null)
                booking.Photos = new List<PhotoUsage>();
            booking.Photos.Add(new PhotoUsage
            {
                PhotoUrl = photoUrl,
                BookingId = booking.Id
            });

            await _context.SaveChangesAsync();
            return true;
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}