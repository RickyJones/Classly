using Classly.Models;
using Classly.Services.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classly.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userServ)
        {
            _userService = userServ;
        }
        public async Task<IActionResult> Index(Guid userId, bool? update = false)
        {
            var user = await _userService.GetUser(userId);

            ViewBag.OwnProfile = user.Id == Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            ViewBag.Update = update;  

            return View(user);
        }

        public async Task<IActionResult> Update(User user)
        {
            var updated = await _userService.UpdateAsync(user, new CancellationToken());

            return RedirectToAction("Index", new { userId = user.Id});
        }
    }
}
