using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Parser.Db;
using Parser.Models;
using Parser.Services;

namespace Parser.Controllers
{
    [Route("[controller]")]
    public class DataController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DataParser _dataParser;
        private readonly IMemoryCache _cache;
        public DataController(AppDbContext context, DataParser dataParser, IMemoryCache cache)
        {
            _context = context;
            _dataParser = dataParser;
            _cache = cache;
        }

        [HttpGet]
        [OutputCache(Duration = 60)]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            _context.ChangeTracker.Clear();

            string cacheKey = $"Purchases_{pageNumber}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<Purchase> purchases))
            {
                if (!_context.Purchases.Any())
                {
                    await _dataParser.ParseAndSaveData("", 5);
                }

                purchases = await _context.Purchases
                                    .AsNoTracking()
                                    .OrderBy(p => p.PurchaseNumber)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

                _cache.Set(cacheKey, purchases, TimeSpan.FromMinutes(30));
            }

            var totalItems = await _context.Purchases.CountAsync();
            var viewModel = new PurchaseViewModel
            {
                Purchases = purchases,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                SearchPhrase = "",
                PageCount = 5
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string searchPhrase, int pageCount = 5, int pageNumber = 1)
        {
            _context.ChangeTracker.Clear();

            string cacheKey = $"Purchases_Search_{searchPhrase}_{pageCount}_{pageNumber}";

            if (!_cache.TryGetValue(cacheKey, out List<Purchase> filteredResults))
            {
                IQueryable<Purchase> query = _context.Purchases;

                if (!string.IsNullOrEmpty(searchPhrase))
                {
                    query = query.Where(p => EF.Functions.ILike(p.Title, $"%{searchPhrase}%"));
                }

                int pageSize = 7;
                var totalItems = await query.CountAsync();
                filteredResults = await query
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

                _cache.Set(cacheKey, filteredResults, TimeSpan.FromMinutes(30));
            }

            var viewModel = new PurchaseViewModel
            {
                Purchases = filteredResults,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)filteredResults.Count / 7),
                SearchPhrase = searchPhrase,
                PageCount = pageCount
            };

            return View(viewModel);
        }


        [HttpGet("api/purchases/all")]
        public async Task<IActionResult> GetAllPurchases()
        {
            string cacheKey = "AllPurchases";

            if (!_cache.TryGetValue(cacheKey, out List<Purchase> purchases))
            {
                purchases = await _context.Purchases.ToListAsync();
                _cache.Set(cacheKey, purchases, TimeSpan.FromMinutes(30));
            }

            return Ok(purchases);
        }
    }
}
