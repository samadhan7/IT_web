using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GTL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IApplication _application;
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly IJobs _jobsRepository;

		public HomeController(IJobs jobs, IApplication application, IWebHostEnvironment hostingEnvironment,ILogger<HomeController> logger)
        {
            _logger = logger;
			_application = application;
			_hostingEnvironment = hostingEnvironment;
			_jobsRepository = jobs;

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
		public async Task<IActionResult> Career()
		{
			try
			{

				var data = await _jobsRepository.GetJobsAsync();

				ViewBag.JobData = data;


				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving jobs data.");
				throw;
			}
		}

		[Route("Career")]
		[HttpPost]
		public async Task<IActionResult> Career(Applications applications, IFormFile resume)
		{
			applications.Resume = null;

			// Validate input fields
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

			var allowedExtensions = new[] { ".jpg", ".jpeg", ".pdf" };
			var fileExtension = Path.GetExtension(resume.FileName).ToLower();

			if (!allowedExtensions.Contains(fileExtension))
			{
				return Json(new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .pdf files are allowed." });
			}


			bool resultMessage = await _application.CheckApplicationAsync(applications.Email,applications.Contact);

			if (!resultMessage) 
			{
				
				var resumeFolderPath = Path.Combine("wwwroot", "assets", "public", "uploads");

				if (!Directory.Exists(resumeFolderPath))
				{
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

					applications.Resume = filePath;

					var result = await _application.AddApplicationAsync(applications);

					return Json(new { success = true, message = result });
				}
				catch (Exception ex)
				{
					return Json(new { success = false, message = "Error while uploading file: " + ex.Message });
				}
			}

			return Json(new { success = false, message = "Application already exists wait for the response." });
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
