using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UssdSessionController : Controller
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<UssdSessionController> _logger;

        public UssdSessionController(DisasterDbContext context, ILogger<UssdSessionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: UssdSession
        public async Task<IActionResult> Index(string searchPhone = "", int pageNumber = 1, int pageSize = 20)
        {
            ViewData["CurrentFilter"] = searchPhone;

            var sessionsQuery = _context.UssdSessions.AsQueryable();

            // Filter by phone number
            if (!string.IsNullOrEmpty(searchPhone))
            {
                sessionsQuery = sessionsQuery.Where(s => s.PhoneNumber.Contains(searchPhone));
            }

            // Get active sessions count
            var activeSessions = await _context.UssdSessions
                .Where(s => s.LastActivityAt > DateTime.UtcNow.AddMinutes(-5))
                .CountAsync();

            ViewData["ActiveSessions"] = activeSessions;

            // Pagination
            var totalSessions = await sessionsQuery.CountAsync();
            var sessions = await sessionsQuery
                .OrderByDescending(s => s.LastActivityAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalSessions"] = totalSessions;
            ViewData["PageNumber"] = pageNumber;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalSessions / (double)pageSize);

            return View(sessions);
        }

        // GET: UssdSession/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.UssdSessions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // GET: UssdSession/Statistics
        public async Task<IActionResult> Statistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");

            var sessions = await _context.UssdSessions
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .ToListAsync();

            // Calculate statistics
            var totalSessions = sessions.Count;
            var uniqueUsers = sessions.Select(s => s.PhoneNumber).Distinct().Count();
            var avgSessionDuration = sessions
                .Where(s => s.LastActivityAt > s.CreatedAt)
                .Select(s => (s.LastActivityAt - s.CreatedAt).TotalSeconds)
                .DefaultIfEmpty(0)
                .Average();

            // Menu state distribution
            var menuStateStats = sessions
                .GroupBy(s => s.CurrentState)
                .Select(g => new { State = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Daily session counts
            var dailyStats = sessions
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            // City distribution
            var cityStats = sessions
                .Where(s => s.CityId != null)
                .GroupBy(s => s.CityId)
                .Select(g => new { CityId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // LGA distribution
            var lgaStats = sessions
                .Where(s => s.LGAId != null)
                .GroupBy(s => s.LGAId)
                .Select(g => new { LGAId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewData["TotalSessions"] = totalSessions;
            ViewData["UniqueUsers"] = uniqueUsers;
            ViewData["AvgDuration"] = Math.Round(avgSessionDuration, 2);
            ViewData["MenuStateStats"] = menuStateStats;
            ViewData["DailyStats"] = dailyStats;
            ViewData["CityStats"] = cityStats;
            ViewData["LGAStats"] = lgaStats;

            return View(sessions);
        }

        // GET: UssdSession/ActiveSessions
        public async Task<IActionResult> ActiveSessions()
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            
            var activeSessions = await _context.UssdSessions
                .Where(s => s.LastActivityAt > fiveMinutesAgo)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync();

            return View(activeSessions);
        }

        // POST: UssdSession/CleanupExpired
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupExpired(int olderThanDays = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
                
                var expiredSessions = await _context.UssdSessions
                    .Where(s => s.LastActivityAt < cutoffDate)
                    .ToListAsync();

                var count = expiredSessions.Count;
                
                _context.UssdSessions.RemoveRange(expiredSessions);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired USSD sessions older than {Days} days", count, olderThanDays);

                TempData["SuccessMessage"] = $"Successfully deleted {count} expired sessions.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired USSD sessions");
                TempData["ErrorMessage"] = "Error cleaning up sessions. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: UssdSession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var session = await _context.UssdSessions.FindAsync(id);
                if (session != null)
                {
                    _context.UssdSessions.Remove(session);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Deleted USSD session {SessionId} for phone {PhoneNumber}", 
                        session.SessionId, session.PhoneNumber);
                    
                    TempData["SuccessMessage"] = "Session deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting USSD session {Id}", id);
                TempData["ErrorMessage"] = "Error deleting session. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: UssdSession/UserSessions/phoneNumber
        public async Task<IActionResult> UserSessions(string phoneNumber, int pageNumber = 1, int pageSize = 20)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewData["PhoneNumber"] = phoneNumber;

            var sessionsQuery = _context.UssdSessions
                .Where(s => s.PhoneNumber == phoneNumber);

            var totalSessions = await sessionsQuery.CountAsync();
            var sessions = await sessionsQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalSessions"] = totalSessions;
            ViewData["PageNumber"] = pageNumber;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalSessions / (double)pageSize);

            return View(sessions);
        }

        // GET: UssdSession/Export
        public async Task<IActionResult> Export(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var sessions = await _context.UssdSessions
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("SessionId,PhoneNumber,CurrentState,UserInput,CityId,LGAId,CreatedAt,LastActivityAt,IsActive,DurationSeconds");

            foreach (var session in sessions)
            {
                var duration = (session.LastActivityAt - session.CreatedAt).TotalSeconds;
                csv.AppendLine($"\"{session.SessionId}\",\"{session.PhoneNumber}\",\"{session.CurrentState}\",\"{session.UserInput?.Replace("\"", "\"\"\"")}\",{session.CityId?.ToString() ?? ""},{session.LGAId?.ToString() ?? ""},\"{session.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{session.LastActivityAt:yyyy-MM-dd HH:mm:ss}\",{session.IsActive},{duration}");
            }

            var fileName = $"ussd_sessions_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }
    }
}
