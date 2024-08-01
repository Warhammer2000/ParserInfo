using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Parser.Db;
using Parser.Models;
using System.Net.Http;

namespace Parser.Services
{
    public class DataParser
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        public DataParser(AppDbContext context, IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
            _cache = cache;
        }

        public async Task ParseAndSaveData(string searchPhrase, int pageCount)
        {
            string cacheKey = $"Purchases_{searchPhrase}_{pageCount}";

            if (_cache.TryGetValue(cacheKey, out List<Purchase> cachedPurchases))
            {
                Console.WriteLine("Данные загружены из кэша.");
                
                _context.Purchases.AddRange(cachedPurchases);
                await _context.SaveChangesAsync();
                return;
            }

            if (_context.Purchases.AsNoTracking().Any())
            {
                Console.WriteLine("Данные уже есть в базе, пропускаем парсинг.");
                return;
            }

            var baseUrl = $"https://www.roseltorg.ru/procedures/search?text={Uri.EscapeDataString(searchPhrase)}";

            var tasks = new List<Task<List<Purchase>>>();

            for (int i = 1; i <= pageCount; i++)
            {
                var url = $"{baseUrl}&page={i}";
                tasks.Add(ProcessPageAsync(url));
            }

            var allPurchases = (await Task.WhenAll(tasks)).SelectMany(p => p).ToList();

            _cache.Set(cacheKey, allPurchases, TimeSpan.FromMinutes(30));

            _context.Purchases.AddRange(allPurchases);
            await _context.SaveChangesAsync();
        }

        private async Task<List<Purchase>> ProcessPageAsync(string url)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(url);
                return ParseHtml(html);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");

                for (int i = 0; i < 40; i++)
                {
                    try
                    {
                        await Task.Delay(2000);
                        var html = await _httpClient.GetStringAsync(url);
                        return ParseHtml(html);
                    }
                    catch (HttpRequestException retryEx)
                    {
                        Console.WriteLine($"Retry {i + 1} failed: {retryEx.Message}");
                    }
                }

                throw new Exception("Failed to retrieve data after multiple attempts.");
            }
        }


        private List<Purchase> ParseHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var purchases = new List<Purchase>();

            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='search-results__item']");
            if (nodes == null) return purchases;

            foreach (var node in nodes)
            {
                var endDateText = GetNodeTextOrDefault(node, ".//time[@class='search-results__time']", "");
                var purchase = new Purchase
                {
                    PurchaseNumber = GetNodeTextOrDefault(node, ".//div[@class='search-results__lot']/a", "Не указан"),
                    Title = GetNodeTextOrDefault(node, ".//div[@class='search-results__subject']/a", "Без названия"),
                    Organizer = GetNodeTextOrDefault(node, ".//div[@class='search-results__customer']/p", "Не указан"),
                    Price = GetNodeTextOrDefault(node, ".//div[@class='search-results__sum']/p", "Цена не указана"),
                    EndDate = ParseDateAsString(endDateText, "Дата не указана"),
                    Location = GetNodeTextOrDefault(node, ".//div[@class='search-results__region']/p", "Местоположение не указано")
                };
                purchases.Add(purchase);
            }

            return purchases;
        }

        private string GetNodeTextOrDefault(HtmlNode node, string xpath, string defaultValue)
        {
            var selectedNode = node.SelectSingleNode(xpath);
            return selectedNode != null && !string.IsNullOrWhiteSpace(selectedNode.InnerText) ? selectedNode.InnerText.Trim() : defaultValue;
        }

        private string ParseDateAsString(string dateText, string defaultValue)
        {
            var cleanDateText = dateText.Split('\n')[0].Trim();
            return !string.IsNullOrEmpty(cleanDateText) ? cleanDateText : defaultValue;
        }
    }

}
