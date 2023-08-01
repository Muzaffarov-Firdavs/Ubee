using Microsoft.AspNetCore.Mvc;
using Ubee.Service.DTOs.Logins;
using Ubee.Service.DTOs.Users;
using Ubee.Service.Interfaces;
using Ubee.Web.Helpers;

namespace Ubee.Web.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService authService;
        private readonly IUserService userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            this.authService = authService;
            this.userService = userService;
        }

        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> PostUserAsync(UserForCreationDto dto)
            => Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = await this.userService.AddUserAsync(dto)
            });

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(LoginDto dto)
            => Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = await this.authService.GenerateTokenAsync(dto.Email, dto.Password)
            });

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync([FromBody] string token)
            => Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = await this.authService.LogoutAsync(token)
            });
    }
}
