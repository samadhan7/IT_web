﻿using Azure;
using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace GTL.Controllers
{
	public class AdminController : Controller
	{
        private readonly ILogger<AdminController> _logger;
        private readonly ILogin _loginRepository;
        private readonly IConfiguration _configuration;
		private readonly IJobs _jobsRepository;
        private readonly IApplication _applicationRepo;
		private readonly IInquiry _inquiryRepo;
		private readonly IUser _userRepo;

		public AdminController(IUser user,IInquiry inquiry, IApplication application, IJobs jobs, ILogin userRepository, IConfiguration configuration, ILogger<AdminController> logger)
        {
            _loginRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
            _jobsRepository = jobs;
            _applicationRepo = application;
			_inquiryRepo = inquiry;
			_userRepo = user;

		}

        [ActionName("Index")]
        public IActionResult Index()
		{
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Index(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
            {
                _logger.LogWarning("Invalid login request received.");
                var ResponseMessage = "Invalid login request";

                TempData["ResponseMessage"] = ResponseMessage;
                return RedirectToAction("Index", "Admin");
            }

            try
            {
                var response = await _loginRepository.LoginUserAsync(request.email, request.password);

                if (response.ResponseCode == 0)
                {
                    var claims = new List<Claim>
                    {
						new Claim("ID", response.UserId.HasValue ? response.UserId.Value.ToString() : string.Empty),
						new Claim(ClaimTypes.Email, request.email),
						new Claim(ClaimTypes.Role, response.Role ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity)
                    );

                    ViewBag.Role = response.Role;  // Use response.Role for the role

                    return RedirectToAction("Dashboard", "Admin");
                }

                TempData["ResponseMessage"] = response.ResponseMessage;
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in user {email}", request.email);
                TempData["ResponseMessage"] = "An error occurred while logging in user"; // Cleaned message

                return RedirectToAction("Index", "Admin");
            }

        }


        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {

			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
			Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
			Response.Headers["Expires"] = "0";

			ClaimsPrincipal currentUser = User;
            var claims = currentUser.Claims;
            var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;


            ViewBag.Role = roleClaim;
            return View();
        }

        [ActionName("Logout")]
        public IActionResult LogoutBasic()
        {
            try
            {
                if (Request.Cookies != null)
                {
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Append(cookie, "", new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(-1)
                        });
                    }
                }

                HttpContext.Session.Clear();

                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction("Index", "Admin");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
                throw;
            }
        }
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Openings()
		{
			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
			Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
			Response.Headers["Expires"] = "0";
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

		[Authorize(Roles = "Admin")]
		[HttpDelete]
		public async Task<IActionResult> Openings(int id)
		{
			try
			{
				// Call your repository method to delete the job by ID
				bool result = await _jobsRepository.DeleteJobAsync(id);

				if (result)
				{
					return Json(new { success = true, message = "Job deleted successfully." });
				}
				else
				{
					return Json(new { success = false, message = "Job not found." });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting jobs data.");
				return Json(new { success = false, message = "Internal Server Error." });
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpPut]
		public async Task<IActionResult> Openings([FromBody] Job jobData, int id)
		{
			try
			{

				bool result = await _jobsRepository.UpdateJobAsync(jobData);

				if (result)
				{
					return Json(new { success = true, message = "Job Updated successfully." });
				}
				else
				{
					return Json(new { success = false, message = "Job not found." });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while Updating jobs data.");
				return Json(new { success = false, message = "Internal Server Error." });
			}
		}

		[Authorize(Roles = "Admin")]
        [HttpPost]
		public async Task<IActionResult> Openings([FromBody] Job jobData)
        {
            if (string.IsNullOrWhiteSpace(jobData.jobName))
            {
                return Json(new { success = false, message = "Job Name is required." });
            }
            if (string.IsNullOrWhiteSpace(jobData.jobDescription))
            {
                return Json(new { success = false, message = "Job Description is required." });
            }
            if (string.IsNullOrWhiteSpace(jobData.location))
            {
                return Json(new { success = false, message = "Job location is required." });
            }
            if (string.IsNullOrWhiteSpace(jobData.experience))
            {
                return Json(new { success = false, message = "Job Experience is required." });
            }
            if (jobData.status != 0 && jobData.status != 1)
            {
                return Json(new { success = false, message = "Status is required." });
            }


			string resultMessage = await _jobsRepository.AddJobAsync(jobData);

			return Json(new { success = true, message = resultMessage });

		}

		[Authorize(Roles = "Admin")]

		public async Task<ActionResult> Applications()
		{
			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
			Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
			Response.Headers["Expires"] = "0";
			try
			{

                var data = await _applicationRepo.GetApplicationsAsync();

				ViewBag.ApplicationsData = data;


				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving applications data.");
				throw;
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete]
		public async Task<ActionResult> Applications(int Id)
		{
			try
			{

				string data = await _applicationRepo.DeleteApplicationAsync(Id);



				return Json(new { success = true, message = data });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting applications data.");
				ViewBag.ResponseMessage = "An error occurred while deleting applications data :" + ex;


				return Json(new { success = false, message = "An error occurred while deleting applications data :" + ex });

			}
		}

		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> Inquiries()
		{
			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
			Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
			Response.Headers["Expires"] = "0";
			try
			{

				var data = await _inquiryRepo.GetInquiriesAsync();

				ViewBag.InquiryData = data;


				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving inquiries data.");
				throw;
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete]
		public async Task<ActionResult> Inquiries(int Id)
		{
			try
			{

				bool data = await _inquiryRepo.DeleteInquiryAsync(Id);

				if (!data) { 
					var response2 = "Somthing went wrong while deleting inquiry.";
					return Json(new { success = true, message = response2 });
				}
				var response = "Inquiry Deleted.";
				return Json(new { success = true, message = response });


			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting inquiry data.");
				var ResponseMessage = "An error occurred while deleting inquiry data :" + ex;


				return Json(new { success = false, message = ResponseMessage });

			}
		}

		[Authorize(Roles = "Admin")]
		public IActionResult Profile()
		{
			Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
			Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
			Response.Headers["Expires"] = "0";

			var emailClaim = User.FindFirst(ClaimTypes.Email);
			string userEmail = emailClaim != null ? emailClaim.Value : "Email not found";

			var idClaim = User.FindFirst("ID");


			string userid = idClaim != null ? idClaim.Value : "id not found";


			ViewBag.UserEmail = userEmail;
			ViewBag.Userid = userid;

			return View();


		}
		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> Profile(string email,string password,int id)
		{

			try
			{
				string ResponseMessage = await _userRepo.UpdateUserAsync(email, password, id);


				TempData["Response"] = ResponseMessage;

				return RedirectToAction("Profile");


			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "An error occurred while updating user data.");
				TempData["Response"] = "An error occurred: " + ex.Message;
				return RedirectToAction("Profile");
			}
			
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
