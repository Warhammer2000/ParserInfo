using Microsoft.EntityFrameworkCore;
using Parser.Db;
using Parser.Interfaces;
using Parser.Models;

namespace Parser.Repository
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDbContext _context;

        public PurchaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Purchase>> GetPurchasesAsync(int pageNumber, int pageSize)
        {
            return await _context.Purchases
                .AsNoTracking()
                .OrderBy(p => p.PurchaseNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Purchase>> SearchPurchasesAsync(string searchPhrase, int pageNumber, int pageSize)
        {
            return await _context.Purchases
                .AsNoTracking()
                .Where(p => EF.Functions.ILike(p.Title, $"%{searchPhrase}%"))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalPurchasesCountAsync()
        {
            return await _context.Purchases.CountAsync();
        }

        public async Task<bool> AnyPurchasesAsync()
        {
            return await _context.Purchases.AnyAsync();
        }

        public async Task AddPurchasesAsync(List<Purchase> purchases)
        {
            _context.Purchases.AddRange(purchases);
            await _context.SaveChangesAsync();
        }

        public async Task ClearAllPurchasesAsync()
        {
            _context.Purchases.RemoveRange(_context.Purchases);
            await _context.SaveChangesAsync();
        }
    }

}
