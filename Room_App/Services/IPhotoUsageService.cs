using System.Collections.Generic;
using System.Threading.Tasks;
using Room_App.Models;
using Room_App.Models;

namespace Room_App.Services
{
    public interface IPhotoUsageService
    {
        Task<IEnumerable<PhotoUsage>> GetAllPhotosAsync();
        Task<IEnumerable<PhotoUsage>> GetPhotosByBookingIdAsync(int bookingId);
        Task<PhotoUsage> GetPhotoByIdAsync(int id);
        Task<PhotoUsage> CreatePhotoAsync(PhotoUsage photo);
        Task<bool> DeletePhotoAsync(int id);
    }
}