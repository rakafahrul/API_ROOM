using System.Collections.Generic;
using System.Threading.Tasks;
using Room_App.Models;

namespace Room_App.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);
        Task<IEnumerable<Booking>> GetBookingsByRoomIdAsync(int roomId);
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> DeleteBookingAsync(int id);
        Task<bool> ApproveBookingAsync(int id, string note);
        Task<bool> RejectBookingAsync(int id, string note);
        Task<bool> CheckinAsync(int id, string locationGps);
        Task<bool> CheckoutAsync(int id, string photoUrl);
    }
}