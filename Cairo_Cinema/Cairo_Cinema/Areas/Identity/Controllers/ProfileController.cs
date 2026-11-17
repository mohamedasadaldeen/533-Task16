using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cairo_Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProfileController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            //var userVM = new ApplicationUserVM();
            //var userVM.Name = user.Name;
            //var userVM.Email = user.Email;
            //var userVM.Address = user.Address;
            //var userVM.Phone = user.PhoneNumber;
            var userVM = user.Adapt<ApplicationUserVM>();
            return View(userVM);
        }
        public async Task<IActionResult> UpdateProfile(ApplicationUserVM applicationUserVM)
        {
            var user = await _userManager.GetUserAsync(User);
            user.UserName = applicationUserVM.UserName;
            user.Name = applicationUserVM.Name;
            user.PhoneNumber = applicationUserVM.PhoneNumber;
            user.Address = applicationUserVM.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("<br>", result.Errors.Select(e => e.Description));
                TempData["Error_Validation"] = errors;
            }
            else
            {
                TempData["Successfully"] = "Update Data Successfully";
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> UpdatePassword(ApplicationUserVM applicationUserVM)
        {

            var user = await _userManager.GetUserAsync(User);
            //كود يفحص اذا حقل الباسورد فارغ 
            if (string.IsNullOrWhiteSpace(applicationUserVM.CurrentPassword) ||
                 string.IsNullOrWhiteSpace(applicationUserVM.NewPassword))
            {
                TempData["Error_Validation"] = "Please enter both passwords.";
                return RedirectToAction(nameof(Index));
            }
            //
            var result = await _userManager.ChangePasswordAsync(user, applicationUserVM.CurrentPassword, applicationUserVM.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("<br>", result.Errors.Select(e => e.Description));
                TempData["Error_Validation"] = errors;
            }
            else
            {
                TempData["Successfully"] = "Update Password Successfully";
            }

            return RedirectToAction(nameof(Index));
        }
        
    }
}
