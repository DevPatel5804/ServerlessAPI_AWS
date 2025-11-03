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
    public class AuthController : ControllerBase
    {
        private readonly UserBusinessObject _userBO;
        private readonly JWTUtilities _jwtService;

        public AuthController(UserBusinessObject user, JWTUtilities jwtService)
        {
            _userBO = user;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var authResponse = await _userBO.LoginUserAsync(request);
                return Ok(authResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred during login.", Error = ex.Message });
            }
        }
    }
}
