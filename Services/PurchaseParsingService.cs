using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Parser.Db;
using Parser.Models;
using System.Net.Http;

namespace Parser.Services
{
    public class PurchaseParsingService
    {
        private readonly HttpClient _httpClient;
        private readonly ParserSettings _settings;

        public PurchaseParsingService(IHttpClientFactory httpClientFactory, IOptions<ParserSettings> settings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
            _settings = settings.Value;
        }

        public async Task<List<Purchase>> ParseDataAsync(string searchPhrase, int pageCount)
        {
            var baseUrl = $"https://www.roseltorg.ru/procedures/search?text={Uri.EscapeDataString(searchPhrase)}";
            var tasks = new List<Task<List<Purchase>>>();

            Parallel.For(1, pageCount + 1, new ParallelOptions { MaxDegreeOfParallelism = _settings.MaxParallelism }, i =>
            {
                var url = $"{baseUrl}&page={i}";
                tasks.Add(ProcessPageAsync(url));
            });

            return (await Task.WhenAll(tasks)).SelectMany(p => p).ToList();
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
                throw;
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
                var purchase = new Purchase
                {
                    PurchaseNumber = GetNodeTextOrDefault(node, ".//div[@class='search-results__lot']/a", "Не указан"),
                    Title = GetNodeTextOrDefault(node, ".//div[@class='search-results__subject']/a", "Без названия"),
                    Organizer = GetNodeTextOrDefault(node, ".//div[@class='search-results__customer']/p", "Не указан"),
                    Price = GetNodeTextOrDefault(node, ".//div[@class='search-results__sum']/p", "Цена не указана"),
                    EndDate = ParseDateAsString(GetNodeTextOrDefault(node, ".//time[@class='search-results__time']", "")),
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

        private string ParseDateAsString(string dateText)
        {
            var cleanDateText = dateText.Split('\n')[0].Trim();
            return !string.IsNullOrEmpty(cleanDateText) ? cleanDateText : "Дата не указана";
        }
    }

}
