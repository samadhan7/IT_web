using Azure;
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

        public AdminController(ILogin userRepository, IConfiguration configuration, ILogger<AdminController> logger)
        {
            _loginRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
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
                new Claim("ID", request.UserId ?? ""),
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
		public IActionResult Openings()
		{
			return View();
		}
		[Authorize(Roles = "Admin")]
		public IActionResult Applications()
		{
			return View();
		}
		[Authorize(Roles = "Admin")]
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
