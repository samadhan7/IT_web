using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Data;
using System.IO;

namespace GTL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IApplication _application;
		private readonly IWebHostEnvironment _hostingEnvironment;

		public HomeController(IApplication application, IWebHostEnvironment hostingEnvironment,ILogger<HomeController> logger)
        {
            _logger = logger;
			_application = application;
			_hostingEnvironment = hostingEnvironment;

		}

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

		[Route("Privacy")]
		public IActionResult Privacy()
        {
            return View();
        }

		[Route("About")]
		public IActionResult About() 
        {
            return View();
        }

		[Route("Service")]
		public IActionResult Service()
		{
			return View();
		}

		[Route("Career")]
		public IActionResult Career()
		{
			return View();
		}

		[Route("Career")]
		[HttpPost]
		public async Task<IActionResult> Career(Applications applications, IFormFile resume)
		{
			applications.Resume = null;

			if (string.IsNullOrWhiteSpace(applications.Name))
			{
				return Json(new { success = false, message = "Name is required." });
			}
			if (string.IsNullOrWhiteSpace(applications.Contact) || applications.Contact.Length != 10 || !applications.Contact.All(char.IsDigit))
			{
				return Json(new { success = false, message = "Invalid Contact number." });
			}
			if (string.IsNullOrWhiteSpace(applications.Address))
			{
				return Json(new { success = false, message = "Address is required." });
			}
			if (string.IsNullOrWhiteSpace(applications.Email))
			{
				return Json(new { success = false, message = "Email is required." });
			}
			if (resume == null)
			{
				return Json(new { success = false, message = "Resume is required." });
			}

			if (resume.Length > 10 * 1024 * 1024)
			{
				return Json(new { success = false, message = "Resume must be less than 10MB." });
			}


			if (resume != null && resume.Length > 0)
			{
				// Validate file extension
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".pdf" };
				var fileExtension = Path.GetExtension(resume.FileName).ToLower();

				if (!allowedExtensions.Contains(fileExtension))
				{
					return Json(new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .pdf files are allowed." });
				}

				var resumeFolderPath = Path.Combine("wwwroot","assets","public","uploads");

				if (!Directory.Exists(resumeFolderPath)) {

					Directory.CreateDirectory(resumeFolderPath);
				}
				

					var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
					var filePath = Path.Combine(resumeFolderPath, uniqueFileName);

				try
				{
					using (FileStream stream = new FileStream(filePath, FileMode.Create))
					{
						await resume.CopyToAsync(stream);
					}

				}
				catch (Exception ex) {
					return Json(new { success = false, message = "Error while uploading file :"+ ex });
				}
					

					applications.Resume = filePath;

					string resultMessage = await _application.AddApplicationAsync(applications);

					return Json(new { success = true, message = resultMessage });

			}

			return Json(new { success = false, message = "Server Error try again" });

		}

		[Route("Contact")]
		public IActionResult Contact()
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
