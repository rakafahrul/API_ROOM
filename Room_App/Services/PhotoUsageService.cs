using Microsoft.EntityFrameworkCore;
using Room_App.Data;
using Room_App.Models;
using Room_App.Services;
using Room_App.Data;
using Room_App.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Room_App.Services
{
    public class PhotoUsageService : IPhotoUsageService
    {
        private readonly ApplicationDbContext _context;

        public PhotoUsageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhotoUsage>> GetAllPhotosAsync()
        {
            return await _context.PhotoUsages.ToListAsync();
        }

        public async Task<IEnumerable<PhotoUsage>> GetPhotosByBookingIdAsync(int bookingId)
        {
            return await _context.PhotoUsages
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<PhotoUsage> GetPhotoByIdAsync(int id)
        {
            return await _context.PhotoUsages.FindAsync(id);
        }

        public async Task<PhotoUsage> CreatePhotoAsync(PhotoUsage photo)
        {
            _context.PhotoUsages.Add(photo);
            await _context.SaveChangesAsync();
            return photo;
        }

        public async Task<bool> DeletePhotoAsync(int id)
        {
            var photo = await _context.PhotoUsages.FindAsync(id);
            if (photo == null)
                return false;

            _context.PhotoUsages.Remove(photo);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}