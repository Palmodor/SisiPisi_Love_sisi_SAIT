using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LoveSushiPMR.Data;
using LoveSushiPMR.Models.Entities;
using LoveSushiPMR.Models.ViewModels;

namespace LoveSushiPMR.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ADMIN_SECRET_CODE = "LOVESUSHI2024_ADMIN";

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            Console.WriteLine($"[LOGIN] Attempt with email: {model.Email}, RememberMe: {model.RememberMe}");

            if (!ModelState.IsValid)
            {
                // Логируем ошибки валидации
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"[LOGIN] Validation error: {error.ErrorMessage}");
                    System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return View(model);
            }

            Console.WriteLine($"[LOGIN] ModelState is valid, searching for user...");

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                Console.WriteLine($"[LOGIN] User with email {model.Email} not found");
                ModelState.AddModelError("", "Неверный email или пароль");
                return View(model);
            }

            Console.WriteLine($"[LOGIN] User found: {user.UserName}, RoleId: {user.RoleId}, Role: {user.Role?.Name}");

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                Console.WriteLine($"[LOGIN] Password hash is empty for user {user.UserName}");
                ModelState.AddModelError("", "Пользователь не был правильно сохранен. Пожалуйста, зарегистрируйтесь заново.");
                return View(model);
            }

            var hashedPassword = HashPassword(model.Password);
            var result = user.PasswordHash == hashedPassword;
            Console.WriteLine($"[LOGIN] Password verification result: {result}");

            if (!result)
            {
                Console.WriteLine($"[LOGIN] Password verification failed");
                ModelState.AddModelError("", "Неверный email или пароль");
                return View(model);
            }

            Console.WriteLine($"[LOGIN] Password verified successfully, signing in user...");
            await SignInUser(user, model.RememberMe);
            Console.WriteLine($"[LOGIN] User signed in successfully");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                Console.WriteLine($"[LOGIN] Redirecting to return URL: {returnUrl}");
                return Redirect(returnUrl);
            }

            if (user.Role?.Name == "Администратор")
            {
                Console.WriteLine($"[LOGIN] User is admin, redirecting to Admin");
                return RedirectToAction("Index", "Admin");
            }

            Console.WriteLine($"[LOGIN] Redirecting to Home");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            Console.WriteLine($"[REGISTER] Registration attempt for email: {model.Email}");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"[REGISTER] Validation error: {error.ErrorMessage}");
                    System.Diagnostics.Debug.WriteLine($"Register validation error: {error.ErrorMessage}");
                }
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                Console.WriteLine($"[REGISTER] User with email {model.Email} already exists");
                ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                return View(model);
            }

            int roleId = 2; // По умолчанию "Пользователь"

            // Проверка, существует ли роль
            var userRole = await _context.AppRoles.FirstOrDefaultAsync(r => r.Name == "Пользователь");
            if (userRole != null)
            {
                roleId = userRole.Id;
                Console.WriteLine($"[REGISTER] Found user role: Id={roleId}");
            }
            else
            {
                Console.WriteLine($"[REGISTER] User role 'Пользователь' not found!");
            }

            // Проверка секретного кода администратора
            if (!string.IsNullOrEmpty(model.AdminCode) && model.AdminCode == ADMIN_SECRET_CODE)
            {
                var adminRole = await _context.AppRoles.FirstOrDefaultAsync(r => r.Name == "Администратор");
                if (adminRole != null)
                {
                    roleId = adminRole.Id;
                    Console.WriteLine($"[REGISTER] Admin code valid, assigning admin role: Id={roleId}");
                }
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                Phone = model.Phone,
                RegistrationDate = DateTime.UtcNow,
                RoleId = roleId,
                EmailConfirmed = true
            };

            user.PasswordHash = HashPassword(model.Password);
            Console.WriteLine($"[REGISTER] Password hashed for user: {user.UserName}");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[REGISTER] User {user.UserName} saved to DB with Id={user.Id}, RoleId={user.RoleId}");

            // Создаём бонусный счёт
            var bonusAccount = new BonusAccount
            {
                UserId = user.Id,
                Balance = 0,
                LastUpdated = DateTime.UtcNow
            };
            _context.BonusAccounts.Add(bonusAccount);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[REGISTER] Bonus account created for user {user.UserName}");

            // Перезагружаем пользователя с ролью
            var registeredUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (registeredUser != null)
            {
                Console.WriteLine($"[REGISTER] User reloaded from DB: Name={registeredUser.Name}, RoleId={registeredUser.RoleId}, Role={registeredUser.Role?.Name}");
                await SignInUser(registeredUser, false);
            }
            else
            {
                Console.WriteLine($"[REGISTER] ERROR: Could not reload user from DB!");
            }

            if (roleId == 1 || registeredUser?.Role?.Name == "Администратор")
            {
                TempData["Success"] = "Аккаунт администратора успешно создан!";
                Console.WriteLine($"[REGISTER] Redirecting to Admin");
                return RedirectToAction("Index", "Admin");
            }

            TempData["Success"] = "Регистрация успешно завершена!";
            Console.WriteLine($"[REGISTER] Redirecting to Home");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login");

            var user = await _context.Users
                .Include(u => u.BonusAccount)
                .Include(u => u.DeliveryAddresses)
                    .ThenInclude(da => da.DeliveryZone)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            var model = new ProfileEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileEditViewModel model)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login");

            if (model.Id != userId)
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Phone = model.Phone;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Профиль успешно обновлен!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> AddDeliveryAddress()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var deliveryZones = await _context.DeliveryZones
                .Where(z => z.IsActive)
                .ToListAsync();

            ViewBag.DeliveryZones = deliveryZones;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDeliveryAddress(DeliveryAddressFormViewModel model)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                var deliveryZones = await _context.DeliveryZones
                    .Where(z => z.IsActive)
                    .ToListAsync();
                ViewBag.DeliveryZones = deliveryZones;
                return View(model);
            }

            // Проверяем существование зоны доставки
            var zoneExists = await _context.DeliveryZones.AnyAsync(z => z.Id == model.DeliveryZoneId && z.IsActive);
            if (!zoneExists)
            {
                var deliveryZones = await _context.DeliveryZones
                    .Where(z => z.IsActive)
                    .ToListAsync();
                ViewBag.DeliveryZones = deliveryZones;
                ModelState.AddModelError("DeliveryZoneId", "Выбранная зона доставки недоступна");
                return View(model);
            }

            // Если выбран адрес по умолчанию, отменяем статус у других
            if (model.IsDefault)
            {
                var otherAddresses = await _context.DeliveryAddresses
                    .Where(da => da.UserId == userId && da.IsDefault)
                    .ToListAsync();

                foreach (var addr in otherAddresses)
                {
                    addr.IsDefault = false;
                    _context.DeliveryAddresses.Update(addr);
                }
            }

            var newAddress = new DeliveryAddress
            {
                UserId = userId,
                City = model.City,
                Street = model.Street,
                House = model.House,
                Apartment = model.Apartment,
                Entrance = model.Entrance,
                Floor = model.Floor,
                Intercom = model.Intercom,
                Comment = model.Comment,
                IsDefault = model.IsDefault,
                DeliveryZoneId = model.DeliveryZoneId
            };

            _context.DeliveryAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Адрес успешно добавлен!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> EditDeliveryAddress(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login");

            var address = await _context.DeliveryAddresses.FirstOrDefaultAsync(da => da.Id == id && da.UserId == userId);
            if (address == null)
                return NotFound();

            var model = new DeliveryAddressFormViewModel
            {
                Id = address.Id,
                City = address.City,
                Street = address.Street,
                House = address.House,
                Apartment = address.Apartment,
                Entrance = address.Entrance,
                Floor = address.Floor,
                Intercom = address.Intercom,
                Comment = address.Comment,
                IsDefault = address.IsDefault,
                DeliveryZoneId = address.DeliveryZoneId
            };

            var deliveryZones = await _context.DeliveryZones
                .Where(z => z.IsActive)
                .ToListAsync();

            ViewBag.DeliveryZones = deliveryZones;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDeliveryAddress(DeliveryAddressFormViewModel model)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login");

            var address = await _context.DeliveryAddresses.FirstOrDefaultAsync(da => da.Id == model.Id && da.UserId == userId);
            if (address == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                var deliveryZones = await _context.DeliveryZones
                    .Where(z => z.IsActive)
                    .ToListAsync();
                ViewBag.DeliveryZones = deliveryZones;
                return View(model);
            }

            // Проверяем существование зоны доставки
            var zoneExists = await _context.DeliveryZones.AnyAsync(z => z.Id == model.DeliveryZoneId && z.IsActive);
            if (!zoneExists)
            {
                var deliveryZones = await _context.DeliveryZones
                    .Where(z => z.IsActive)
                    .ToListAsync();
                ViewBag.DeliveryZones = deliveryZones;
                ModelState.AddModelError("DeliveryZoneId", "Выбранная зона доставки недоступна");
                return View(model);
            }

            // Если выбран адрес по умолчанию, отменяем статус у других
            if (model.IsDefault && !address.IsDefault)
            {
                var otherAddresses = await _context.DeliveryAddresses
                    .Where(da => da.UserId == userId && da.IsDefault && da.Id != model.Id)
                    .ToListAsync();

                foreach (var addr in otherAddresses)
                {
                    addr.IsDefault = false;
                    _context.DeliveryAddresses.Update(addr);
                }
            }

            address.City = model.City;
            address.Street = model.Street;
            address.House = model.House;
            address.Apartment = model.Apartment;
            address.Entrance = model.Entrance;
            address.Floor = model.Floor;
            address.Intercom = model.Intercom;
            address.Comment = model.Comment;
            address.IsDefault = model.IsDefault;
            address.DeliveryZoneId = model.DeliveryZoneId;

            _context.DeliveryAddresses.Update(address);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Адрес успешно обновлен!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDeliveryAddress(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login");

            var address = await _context.DeliveryAddresses.FirstOrDefaultAsync(da => da.Id == id && da.UserId == userId);
            if (address == null)
                return NotFound();

            _context.DeliveryAddresses.Remove(address);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Адрес успешно удален!";
            return RedirectToAction("Profile");
        }

        private async Task SignInUser(User user, bool rememberMe)
        {
            Console.WriteLine($"[SIGNIN] Starting sign in for user: {user.UserName}, RoleId: {user.RoleId}, Role: {user.Role?.Name}");
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Пользователь")
            };

            Console.WriteLine($"[SIGNIN] Claims created: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
            };

            Console.WriteLine($"[SIGNIN] Creating HTTP context sign in...");
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            
            Console.WriteLine($"[SIGNIN] Sign in completed successfully");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
