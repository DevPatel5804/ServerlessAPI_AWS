using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using serverless_auth.BusinessObjects;
using serverless_auth.Utilities;
using serverless_auth.ViewModels;
using System;
using System.Threading.Tasks;

namespace serverless_auth.Controllers
{
    [Route("jwt-auth/api/[controller]")]
    [ApiController]
    [XApiKeyUtilities]
    public class UserController : ControllerBase
    {
        private readonly UserBusinessObject _userBO;

        public UserController(UserBusinessObject user)
        {
            _userBO = user;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddOrUpdateUser([FromBody] AddUserViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userBO.AddOrUpdateUserAsync(request);
                return Ok(new { Message = "User added/updated successfully.", Email = user.UserID, ApplicationId = user.ApplicationID });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during user add/update: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred during user add/update.", Error = ex.Message });
            }
        }
    }
}
