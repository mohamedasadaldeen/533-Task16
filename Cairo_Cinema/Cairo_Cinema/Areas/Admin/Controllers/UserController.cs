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
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserVM
            {
                Roles = _roleManager.Roles.Select(r => r.Name!).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            vm.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();

            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                Email = vm.Email,
                Name = vm.Name,
                PhoneNumber = vm.PhoneNumber,
                Address = vm.Address,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                TempData["Error_Validation"] = string.Join("<br>", result.Errors.Select(e => e.Description));
                return View(vm);
            }

            if (!string.IsNullOrEmpty(vm.Role))
                await _userManager.AddToRoleAsync(user, vm.Role);

            TempData["Successfully"] = "User Created Successfully";
            return RedirectToAction(nameof(Create));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = CD.SUPER_ADMIN)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            // منع حذف نفسك
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error_Validation"] = "You can't delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // منع حذف السوبر أدمن
            if (await _userManager.IsInRoleAsync(user, CD.SUPER_ADMIN))
            {
                TempData["Error_Validation"] = "You can't delete a Super Admin.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error_Validation"] = string.Join("<br>", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["Successfully"] = "User Deleted Successfully";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}

