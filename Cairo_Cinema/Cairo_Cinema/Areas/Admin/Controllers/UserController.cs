using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Collections.Specialized.BitVector32;

namespace Cairo_Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =$"{CD.SUPER_ADMIN}, {CD.ADMIN}")]
    public class UserController: Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        //Index
        public async Task<IActionResult> Index()
        {
            var users =  _userManager.Users;
            return View(users);
        }

        //Index
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            //Super Admin UnLock Always
            var isSuperAdmin = await _userManager.IsInRoleAsync(user, CD.SUPER_ADMIN);
            if (isSuperAdmin)
            {
                TempData["Error_Validation"] = "You Can't Lock Super Admin !";
                return RedirectToAction(nameof(Index));
            }

            //UnLock
            if (user.LockoutEnd == null || DateTime.UtcNow > user.LockoutEnd)
            {
                await _userManager.SetLockoutEndDateAsync(user,DateTime.UtcNow.AddDays(5));
            }
            else
            {
                 await _userManager.SetLockoutEndDateAsync(user, null);

            }

            return RedirectToAction(nameof(Index));
        }
    }
}

