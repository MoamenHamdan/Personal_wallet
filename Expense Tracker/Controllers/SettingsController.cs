using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Expense_Tracker.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var displayName = Request.Cookies["DisplayName"] ?? "Moamen Hamdan";
            return View("Index", displayName);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings(string displayName, string theme, string currency, IFormFile profilePicture)
        {
            if (!string.IsNullOrEmpty(displayName))
            {
                Response.Cookies.Append("DisplayName", displayName, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1)
                });
            }

            if (!string.IsNullOrEmpty(theme))
            {
                Response.Cookies.Append("Theme", theme, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1)
                });
            }

            if (!string.IsNullOrEmpty(currency))
            {
                Response.Cookies.Append("Currency", currency, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1)
                });
            }

            if (profilePicture != null)
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
                string filePath = Path.Combine(uploadsDir, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(fileStream);
                }
                Response.Cookies.Append("ProfilePicture", uniqueFileName, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1)
                });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ResetSettings()
        {
            Response.Cookies.Delete("DisplayName");
            Response.Cookies.Delete("Theme");
            Response.Cookies.Delete("ProfilePicture");
            Response.Cookies.Delete("Currency");

            return RedirectToAction("Index");
        }
    }
} 