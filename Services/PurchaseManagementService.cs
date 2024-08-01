using Microsoft.EntityFrameworkCore;
using Parser.Db;

namespace Parser.Services
{
    public class PurchaseManagementService
    {
        private readonly AppDbContext _context;

        public PurchaseManagementService(AppDbContext appDb)
        {
            _context = appDb;
        }
        public async Task DeleteAllDataAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Purchases.RemoveRange(_context.Purchases);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    Console.WriteLine($"Ошибка при удалении данных: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
