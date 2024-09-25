using GTL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GTL.Controllers
{
	public class AdminController : Controller
	{
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Dashboard()
        {
            return View();
        }

		public IActionResult Openings()
		{
			return View();
		}

		public IActionResult Applications()
		{
			return View();
		}

		public IActionResult Inquiries()
		{
			return View();
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
