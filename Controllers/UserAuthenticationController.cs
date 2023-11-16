﻿using NewLagosFloodDetectionSystem.AccountRepository.Abstract;
using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using NewLagosFloodDetectionSystem.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
//using NuGet.Protocol.Plugins;
using System.Net.Mail;
using System.Security.Claims;

namespace NewLagosFloodDetectionSystem.Controllers
{
    public class UserAuthenticationController : Controller
    {
        private readonly IUserAuthenticationService _service;
        private readonly FloodDbContext _context;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IServiceProvider _serviceProvider;


        public UserAuthenticationController(IUserAuthenticationService service, FloodDbContext context, 
            SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IServiceProvider serviceProvider)
        {
            _service = service;
            _context = context;
            this.signInManager = signInManager;
            _userManager = userManager;
            this.roleManager = roleManager;
            _serviceProvider = serviceProvider;

        }
        public IActionResult Register()
        {
            var lgas = _context.LGAs
                .Where(x => x.LGAName != "None")
                .ToList();
            var lgaListItems = lgas.Select(c => new SelectListItem
            {
                Value = c.LGAId.ToString(),
                Text = c.LGAName
            }).ToList();
            ViewBag.lgas = lgaListItems;
            return View();
        }
        public JsonResult GetCitiesByLGA(int lGAId)
        {
            var cities = _context.Cities
                .Where(p => p.LGAId == lGAId)
                .Select(p => new { p.Id, p.Name })
                .ToList();
            return Json(cities);
        }
        public JsonResult GetLGAs(int lGAId)
        {
            var lgas = _context.LGAs.ToList();
            return Json(lgas);
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegistrationModel model)
        {
            var status = new Status();
            if (!ModelState.IsValid)
                return Register();
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                status.StatusCode = 0;
                status.Message = "User already exists";
            }
            var lga = await _context.LGAs.FindAsync(model.LGAId);
            var lgaName = lga.LGAName;
            var city = await _context.Cities.FindAsync(model.CityId);
            var cityName = city.Name;

            var user = new User
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true,
                LGAId = model.LGAId,
                CityId = model.CityId,
                LGA = lgaName,
                City = cityName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "User creation failed";
            }
            model.Role = "user";

            if (!await roleManager.RoleExistsAsync(model.Role))
                await roleManager.CreateAsync(new IdentityRole(model.Role));

            if (await roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            status.StatusCode = 1;
            status.Message = "User has registered successfully!";

            return RedirectToAction(nameof(Login));
        }
        public async Task<string> GetCurrentUserIdAsync(ClaimsPrincipal user)
        {
            var currentUser = await _userManager.GetUserAsync(user);
            return currentUser.Id;
        }

        [HttpGet]
        public async Task<IActionResult> Detail()
        {
            var userId = await GetCurrentUserIdAsync(User);
            var user = _context.Users.Find(userId);
            var lga = await _context.LGAs.FindAsync(user.LGAId);
            var lgaName = lga.LGAName;
            var city = await _context.Cities.FindAsync(user.CityId);
            var cityName = city.Name;
            // Use the userId to retrieve the user details from the database and pass them to the view.
            var userDetails = new User
            {
                 UserName = user.UserName,
                 Email = user.Email,
                 LGA = lgaName,
                 City = cityName,
            };
            return View(userDetails);
        }


        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var result = await _service.LoginAsync(login);
            if(result.StatusCode == 1)
            {
                if (User.IsInRole("admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }else if (!User.IsInRole("admin"))
                {
                   
                    return RedirectToAction("Index", "Home");
                }
                return View(login);
            }
            else
            {
                TempData["msg"] = result.Message;
                return RedirectToAction(nameof(Login));
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _service.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> RegAdmin()
        {
            var lgas = await _context.LGAs.ToListAsync();
            var cities = await _context.Cities.ToListAsync();
            if(lgas.FirstOrDefault(x => x.LGAName == "None") == null)
            {
                var lga = new LGA
                {
                    LGAName = "None"
                };
                _context.LGAs.Add(lga);
                await _context.SaveChangesAsync();
                if(cities.FirstOrDefault(c => c.Name == "None") == null)
                {
                    var city = new City
                    {
                        LGAId = lga.LGAId,
                        Name = "None"
                    };
                    _context.Cities.Add(city);
                    await _context.SaveChangesAsync();

                }
            }
            var model = new RegistrationModel
            {
                UserName = "AdminUser",
                Email = "admin@gmail.com",
                Password = "pA$$w0rd"
            };
            model.Role = "admin";
            var result = await _service.RegistrationAdminAsync(model);
            return Ok(result);
        }

        public async Task<IActionResult> Edit()
        {
            var lgas = _context.LGAs
                .Where(x => x.LGAName != "None")
                .ToList();
            var lgaListItems = lgas.Select(c => new SelectListItem
            {
                Value = c.LGAId.ToString(),
                Text = c.LGAName
            }).ToList();
            ViewBag.lgas = lgaListItems;
            var id = await GetCurrentUserIdAsync(User);
            var user = _context.Users.Find(id);
            var users = _context.Users.Where(x => x.Id == user.Id).FirstOrDefault();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserEditViewModel userEdit)
        {
            var id = await GetCurrentUserIdAsync(User);
            var user = _context.Users.Find(id);
            var lga = await _context.LGAs.FindAsync(userEdit.LGAId);
            var lgaName = lga.LGAName;
            var city = await _context.Cities.FindAsync(userEdit.CityId);
            var cityName = city.Name;

            user.UserName = userEdit.UserName;
            user.Email = userEdit.Email;
            user.LGAId = userEdit.LGAId;
            user.CityId = userEdit.CityId;
            user.LGA = lgaName;
            user.City = cityName;
            await _userManager.UpdateNormalizedUserNameAsync(user);
            await _userManager.UpdateNormalizedEmailAsync(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Detail));

            
        }

        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword pwdEdit)
        {
            var id = await GetCurrentUserIdAsync(User);
            var existingUser = _context.Users.Find(id);
            var hasher = new PasswordHasher<User>();
            if (hasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, pwdEdit.OldPassword) == PasswordVerificationResult.Success)
            {
                existingUser.PasswordHash = hasher.HashPassword(existingUser, pwdEdit.NewPassword);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Detail));
            }
            return View();
        }

        private void CheckStatusAndSendEmail(CityFloodPrediction entity)
        {
            if (entity.Prediction == true.ToString())
            {
                var users = _context.Users.Where(x => x.CityId == entity.CityId).ToList();
                // send email to user
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("karinateidoutimiwei@gmail.com");
                foreach (var user in users)
                {
                    mail.To.Add(user.Email);
                    mail.Subject = "Flood Risk In Your City";
                    mail.Body = $"Dear {user.UserName}, Your city, {entity.City} is predicted to have a high flood risk in {entity.Month}.";
                }

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("karinateidoutimiwei@gmail.com", "puhziotjpdotvrtj");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
        }


    }
}
