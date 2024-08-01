using Microsoft.AspNetCore.Mvc;
using Parser.Models;
using Parser.Services;
using System.Diagnostics;

namespace Parser.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataService _dataService;
        public HomeController(ILogger<HomeController> logger, DataService data)
        {
            _logger = logger;
            _dataService = data;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> DeleteAllData()
        {
            await _dataService.DeleteAllDataAsync();
            return RedirectToAction("Index");
        }
    }
}
