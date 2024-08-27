using HW_mvc1.DAL;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Services.Implementations
{
    public class LayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, string>> GetSettings()
        {
            return await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
        }

    }
}
