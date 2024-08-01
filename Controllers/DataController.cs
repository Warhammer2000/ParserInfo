using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Parser.Interfaces;
using Parser.Models;
using Parser.Services;

/*
 * IMemoryCache https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-8.0
 * Output Cache (Output Caching) https://learn.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-8.0
 
IMemoryCache используется для кэширования данных на уровне сервиса или логики приложения.
Output Cache используется для кэширования готового ответа, возвращаемого клиенту.

Уровень кэширования:
IMemoryCache работает на уровне кода, внутри приложения.
Output Cache работает на уровне HTTP-ответа, который возвращается клиенту.

Гибкость:
IMemoryCache более гибкий и предоставляет разработчику контроль над тем,
какие данные кэшировать и как долго их хранить.
Output Cache проще в использовании для кэширования целых страниц или API-ответов,
но с меньшей гибкостью.
*/
namespace Parser.Controllers                                                                                   
{
    [Route("[controller]")]
    public class DataController : Controller
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly PurchaseParsingService _parsingService;
        private readonly IMemoryCache _cache;
        private readonly ParserSettings _settings;

        public DataController(IPurchaseRepository purchaseRepository, PurchaseParsingService parsingService,
            IMemoryCache cache, IOptions<ParserSettings> settings)
        {
            _purchaseRepository = purchaseRepository;
            _parsingService = parsingService;
            _cache = cache;
            _settings = settings.Value;
        }

        [HttpGet]
        [OutputCache(Duration = 60)]
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            int pageSize = _settings.DefaultPageSize;
            string cacheKey = CacheKeys.GetPurchases(pageNumber, pageSize);

            if (!_cache.TryGetValue(cacheKey, out List<Purchase> purchases))
            {
                if (!await _purchaseRepository.AnyPurchasesAsync())
                {
                    var parsedData = await _parsingService.ParseDataAsync("", 5);
                    await _purchaseRepository.AddPurchasesAsync(parsedData);
                }

                purchases = await _purchaseRepository.GetPurchasesAsync(pageNumber, pageSize);
                _cache.Set(cacheKey, purchases, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
            }

            var totalItems = await _purchaseRepository.GetTotalPurchasesCountAsync();
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
            string cacheKey = CacheKeys.SearchPurchases(searchPhrase, pageCount, pageNumber); 

            if (!_cache.TryGetValue(cacheKey, out List<Purchase> filteredResults))
            {
                filteredResults = await _purchaseRepository.SearchPurchasesAsync(searchPhrase, pageNumber, 7);
                _cache.Set(cacheKey, filteredResults, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
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

        [HttpGet("api/purchases")]
        public async Task<IActionResult> GetAllPurchases()
        {
            string cacheKey = CacheKeys.AllPurchases;

            if (!_cache.TryGetValue(cacheKey, out List<Purchase> purchases))
            {
                purchases = await _purchaseRepository.GetPurchasesAsync(1, int.MaxValue);
                _cache.Set(cacheKey, purchases, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
            }

            return Ok(purchases);
        }
    }
}
