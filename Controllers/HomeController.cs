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
		private readonly IInquiry _inquiryRepository;

		public HomeController(IInquiry inquiry, IJobs jobs, IApplication application, IWebHostEnvironment hostingEnvironment,ILogger<HomeController> logger)
        {
            _logger = logger;
			_application = application;
			_hostingEnvironment = hostingEnvironment;
			_jobsRepository = jobs;
			_inquiryRepository = inquiry;
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
		[ActionName("Contactform")]
		[HttpPost]
		public async Task<IActionResult> Contact(Inquiry inquiry)
		{
			if (string.IsNullOrWhiteSpace(inquiry.name))
			{
				TempData["Response"] = "Name is required.";
				return RedirectToAction("Contact");

			}

			// Validate Email
			if (string.IsNullOrWhiteSpace(inquiry.email) || !IsValidEmail(inquiry.email))
			{
				TempData["Response"] = "Valid email is required.";
				return RedirectToAction("Contact");

			}
			if (string.IsNullOrWhiteSpace(inquiry.contact) || inquiry.contact.Length != 10 || !inquiry.contact.All(char.IsDigit))
			{

			
				var ResponseMessage = "Contact number is invalid enter valid one.";

				TempData["Response"] = ResponseMessage;
				return RedirectToAction("Contact");

			}
			if (string.IsNullOrWhiteSpace(inquiry.subject))
			{
				TempData["Response"] = "Subject is required.";
				return RedirectToAction("Contact");
			}

			// Validate Message
			if (string.IsNullOrWhiteSpace(inquiry.message))
			{
				TempData["Response"] = "Message is required.";
				return RedirectToAction("Contact");
			}
			try
			{
				var ResponseMessage = await _inquiryRepository.AddInquiryAsync(inquiry);
				TempData["Response"] = ResponseMessage;

				return RedirectToAction("Contact");
			}
			catch(Exception ex)
			{
				TempData["Response"] = "Server Error :"+ ex;
				
			}
			return RedirectToAction("Contact");

		}

		private bool IsValidEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
