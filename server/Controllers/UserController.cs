
using app.Model;
using app.Model.DBase;
using app.Model.DTOs.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<ApplicationUser> getUser()
        {
            return await _userManager.FindByEmailAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        [HttpGet]
        [Route("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var jmenu = new List<JsonMenu>();
            var user = await getUser();
            var userSettings = await _context.UserSettings.Where(a => a.UserId == user.Id).Include(b => b.Setting).ThenInclude(c => c.UserMenu).FirstOrDefaultAsync();
            if (userSettings == null)
                return BadRequest( new UserResponse { 
                    Success=false,
                    Message = new List<string> { "User haven't menu" }
                });

            userSettings.Setting.UserMenu.JsonMenu.Split(';').ToList().ForEach(a => jmenu.Add(JsonConvert.DeserializeObject<JsonMenu>(a.ToString())));
            return Ok( new UserResponse { 
                Success = true,
                JsonMenu = jmenu
            });
        }

        [HttpGet]
        [Route("workshift")]
        public async Task<IActionResult> GetWorkShift()
        {
            var user = await getUser();
            var workShift = await _context.UserWorkShifts.Where(a => a.UserId == user.Id && a.Status == true).Include(b => b.User).FirstOrDefaultAsync();
            if (workShift != null)
                return Ok(new UserResponse { 
                    Success = true,
                    UserInfo = new { workShift.User.FullName, workShift.NumberOfWorkShift }
                });

            return Ok(new UserResponse {
                    Success = true,
                    UserInfo = new { user.FullName }
            });
        }
    }
}
