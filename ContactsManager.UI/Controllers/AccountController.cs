using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Enums;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace ContactsManager.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize("NotAuthorized")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize("NotAuthorized")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(registerDTO);
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                PhoneNumber = registerDTO.Phone,
                PersonName = registerDTO.PersonName
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                // Check status of radio button
                if (registerDTO.UserType == UserTypeOptions.Admin)
                {
                    // Create Admin Role
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) == null)
                    {
                        ApplicationRole role = new ApplicationRole { Name = UserTypeOptions.Admin.ToString() };
                        await _roleManager.CreateAsync(role);
                    }

                    // Add user to Admin Role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());
                }
                else
                {
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.User.ToString()) == null)
                    {
                        ApplicationRole role = new ApplicationRole { Name = UserTypeOptions.User.ToString() };
                        await _roleManager.CreateAsync(role);
                    }

                    // Add user to User Role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());
                }

                // Sign In
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("Register", error.Description);
            }
            //ViewBag.Errors = result.Errors.Select(e => e.Description).ToList();

            return View(registerDTO);
        }
        [HttpGet]
        [Authorize("NotAuthorized")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Authorize("NotAuthorized")]
        public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(loginDTO);
            }

            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: loginDTO.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(loginDTO.Email);

                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                    {
                        return RedirectToAction("Dashboard", "Persons");
                    }
                }

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }
                return RedirectToAction(nameof(PersonsController.Dashboard), "Persons");
            }

            if (result.IsLockedOut)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(loginDTO.Email);
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                if (lockoutEnd.HasValue)
                {
                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    DateTime lockoutEndLocal = TimeZoneInfo.ConvertTimeFromUtc(lockoutEnd.Value.UtcDateTime, egyptTimeZone);
                    var remainingTime = lockoutEndLocal - DateTime.Now;

                    ModelState.AddModelError("Login",
                        $"Account is locked out until {lockoutEndLocal:HH:mm:ss}." +
                        $"Try again in {remainingTime.Minutes} minutes.");
                }
                else
                {
                    ModelState.AddModelError("Login", "Account is locked out. Try again later.");
                }
                return View(loginDTO);
            }

            ModelState.AddModelError("Login", "Invalid email or password");

            return View(loginDTO);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }

        [AllowAnonymous]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);
            return Json(user == null);
        }
    }
}
