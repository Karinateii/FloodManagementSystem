using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;
using GlobalDisasterManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize]
    public class ShelterController : Controller
    {
        private readonly IShelterService _shelterService;
        private readonly ILogger<ShelterController> _logger;
        private readonly DisasterDbContext _context;

        public ShelterController(IShelterService shelterService, ILogger<ShelterController> logger, DisasterDbContext context)
        {
            _shelterService = shelterService;
            _logger = logger;
            _context = context;
        }

        // GET: Shelter/Index
        public async Task<IActionResult> Index(string searchTerm, int? cityFilter, int? lgaFilter, string statusFilter, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.EmergencyShelters
                    .Include(s => s.City)
                    .Include(s => s.LGA)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(s => s.Name.Contains(searchTerm) || 
                                           s.Address.Contains(searchTerm));
                }

                if (cityFilter.HasValue)
                {
                    query = query.Where(s => s.CityId == cityFilter.Value);
                }

                if (lgaFilter.HasValue)
                {
                    query = query.Where(s => s.LGAId == lgaFilter.Value);
                }

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    switch (statusFilter.ToLower())
                    {
                        case "active":
                            query = query.Where(s => s.IsActive);
                            break;
                        case "inactive":
                            query = query.Where(s => !s.IsActive);
                            break;
                        case "operational":
                            query = query.Where(s => s.IsOperational);
                            break;
                        case "available":
                            query = query.Where(s => s.CurrentOccupancy < s.TotalCapacity);
                            break;
                        case "full":
                            query = query.Where(s => s.CurrentOccupancy >= s.TotalCapacity);
                            break;
                    }
                }

                // Get total count for pagination
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Apply pagination
                var shelters = await query
                    .OrderBy(s => s.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Populate filter dropdowns
                var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
                var lgas = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();

                ViewBag.Cities = new SelectList(cities, "Id", "Name", cityFilter);
                ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName", lgaFilter);
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CityFilter = cityFilter;
                ViewBag.LGAFilter = lgaFilter;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;

                return View(shelters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shelters");
                TempData["Error"] = "An error occurred while loading shelters.";
                return View(new List<EmergencyShelter>());
            }
        }

        // GET: Shelter/FindNearby
        public IActionResult FindNearby()
        {
            return View();
        }

        // POST: Shelter/FindNearby
        [HttpPost]
        public async Task<IActionResult> FindNearby(double latitude, double longitude, double radius = 10)
        {
            try
            {
                var shelters = await _shelterService.GetNearbySheltersAsync(latitude, longitude, radius);
                ViewBag.UserLocation = new { Latitude = latitude, Longitude = longitude, Radius = radius };
                return View("NearbyResults", shelters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby shelters");
                TempData["Error"] = "An error occurred while searching for shelters.";
                return View();
            }
        }

        // GET: Shelter/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var shelter = await _shelterService.GetShelterByIdAsync(id);
                if (shelter == null)
                {
                    TempData["Error"] = "Shelter not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(shelter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shelter details");
                TempData["Error"] = "An error occurred while loading shelter details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Shelter/Edit/5 - Admin only
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var shelter = await _shelterService.GetShelterByIdAsync(id);
                if (shelter == null)
                {
                    TempData["Error"] = "Shelter not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Populate dropdowns
                var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
                var lgas = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();

                ViewBag.Cities = new SelectList(cities, "Id", "Name", shelter.CityId);
                ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName", shelter.LGAId);

                return View(shelter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shelter for edit");
                TempData["Error"] = "An error occurred while loading the shelter.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shelter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(Guid id, EmergencyShelter shelter)
        {
            if (id != shelter.Id)
            {
                TempData["Error"] = "Invalid shelter ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _shelterService.UpdateShelterAsync(shelter);
                    TempData["Success"] = "Shelter updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = shelter.Id });
                }

                // Repopulate dropdowns on validation failure
                var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
                var lgas = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();

                ViewBag.Cities = new SelectList(cities, "Id", "Name", shelter.CityId);
                ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName", shelter.LGAId);

                return View(shelter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shelter");
                TempData["Error"] = "An error occurred while updating the shelter.";

                // Repopulate dropdowns
                var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
                var lgas = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();

                ViewBag.Cities = new SelectList(cities, "Id", "Name", shelter.CityId);
                ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName", shelter.LGAId);

                return View(shelter);
            }
        }

        // GET: Shelter/CheckIn/5
        public async Task<IActionResult> CheckIn(Guid shelterId)
        {
            try
            {
                var shelter = await _shelterService.GetShelterByIdAsync(shelterId);
                if (shelter == null)
                {
                    TempData["Error"] = "Shelter not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (shelter.CurrentOccupancy >= shelter.TotalCapacity)
                {
                    TempData["Error"] = "This shelter is at full capacity.";
                    return RedirectToAction(nameof(Details), new { id = shelterId });
                }

                var checkIn = new ShelterCheckIn { ShelterId = shelterId };
                ViewBag.ShelterName = shelter.Name;

                return View(checkIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading check-in form");
                TempData["Error"] = "An error occurred while loading the check-in form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shelter/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(ShelterCheckIn checkIn)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(checkIn);
                }

                await _shelterService.CheckInToShelterAsync(checkIn);

                TempData["Success"] = $"Successfully checked in {checkIn.FamilyMembers} person(s) to the shelter.";
                return RedirectToAction(nameof(Details), new { id = checkIn.ShelterId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing check-in");
                TempData["Error"] = "An error occurred while processing check-in.";
                return View(checkIn);
            }
        }

        // GET: Shelter/Map
        public async Task<IActionResult> Map()
        {
            try
            {
                var shelters = await _shelterService.GetActiveSheltersAsync();
                return View(shelters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shelter map");
                TempData["Error"] = "An error occurred while loading the map.";
                return View(new List<EmergencyShelter>());
            }
        }

        // GET: Shelter/Create - Admin only
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.ShelterTypes = Enum.GetValues(typeof(ShelterType))
                .Cast<ShelterType>()
                .Select(st => new { Value = (int)st, Text = st.ToString() })
                .ToList();

            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            var lgas = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();

            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName");

            return View();
        }

        // POST: Shelter/Create - Admin only
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmergencyShelter shelter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Repopulate dropdowns
                    ViewBag.ShelterTypes = Enum.GetValues(typeof(ShelterType))
                        .Cast<ShelterType>()
                        .Select(st => new { Value = (int)st, Text = st.ToString() })
                        .ToList();

                    var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
                    var lgas = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();

                    ViewBag.Cities = new SelectList(cities, "Id", "Name");
                    ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName");

                    return View(shelter);
                }

                await _shelterService.CreateShelterAsync(shelter);

                TempData["Success"] = "Emergency shelter created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shelter");
                TempData["Error"] = "An error occurred while creating the shelter.";
                
                // Repopulate dropdowns
                ViewBag.ShelterTypes = Enum.GetValues(typeof(ShelterType))
                    .Cast<ShelterType>()
                    .Select(st => new { Value = (int)st, Text = st.ToString() })
                    .ToList();

                var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
                var lgas = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();

                ViewBag.Cities = new SelectList(cities, "Id", "Name");
                ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName");
                
                return View(shelter);
            }
        }

        // GET: Shelter/Occupants/5 - Admin only
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Occupants(Guid shelterId)
        {
            try
            {
                var shelter = await _shelterService.GetShelterByIdAsync(shelterId);
                if (shelter == null)
                {
                    TempData["Error"] = "Shelter not found.";
                    return RedirectToAction(nameof(Index));
                }

                var occupants = await _shelterService.GetShelterOccupantsAsync(shelterId);
                ViewBag.Shelter = shelter;

                return View(occupants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shelter occupants");
                TempData["Error"] = "An error occurred while loading occupants.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
