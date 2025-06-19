using Microsoft.EntityFrameworkCore;
using Room_App.Data;


namespace Room_App.Utility
{
    public class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            await dbContextSvc.Database.MigrateAsync();
        }
    }
}
