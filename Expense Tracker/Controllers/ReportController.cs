using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Expense_Tracker.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string? dateRange, string? transactionType, int? categoryId)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.Split(" - ");
                if (dates.Length == 2)
                {
                    startDate = DateTime.Parse(dates[0]);
                    endDate = DateTime.Parse(dates[1]);
                }
            }

            ViewData["startDate"] = startDate;
            ViewData["endDate"] = endDate;
            ViewData["transactionType"] = transactionType;
            ViewData["categoryId"] = categoryId;

            var query = _context.Transactions.Include(t => t.Category).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(transactionType) && transactionType != "All")
            {
                query = query.Where(t => t.Category.Type == transactionType);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            var transactions = await query.ToListAsync();

            // Calculate summary data
            int totalIncome = transactions.Where(t => t.Category?.Type == "Income").Sum(t => t.Amount);
            int totalExpense = transactions.Where(t => t.Category?.Type == "Expense").Sum(t => t.Amount);
            int balance = totalIncome - totalExpense;

            ViewBag.TotalIncome = totalIncome.ToString("C0");
            ViewBag.TotalExpense = totalExpense.ToString("C0");
            ViewBag.Balance = balance.ToString("C0");

            // Doughnut Chart Data
            ViewBag.DoughnutChartData = transactions
                .Where(i => i.Category?.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            // Load font for PDF export
            string fontPath = Path.Combine(_webHostEnvironment.WebRootPath, "fonts", "arial.ttf");
            if (System.IO.File.Exists(fontPath))
            {
                byte[] fontBytes = await System.IO.File.ReadAllBytesAsync(fontPath);
                ViewBag.PdfFont = Convert.ToBase64String(fontBytes);
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(transactions);
        }
    }
} 