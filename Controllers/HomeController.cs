using Microsoft.AspNetCore.Mvc;
using Parser.Models;
using Parser.Services;
using System.Diagnostics;

namespace Parser.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PurchaseManagementService _purchaseService;
        public HomeController(ILogger<HomeController> logger, PurchaseManagementService purchaseService)
        {
            _logger = logger;
            _purchaseService = purchaseService;
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
            await _purchaseService.DeleteAllDataAsync();
            return RedirectToAction("Index");
        }
    }
}
