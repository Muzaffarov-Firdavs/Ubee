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
        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> PostUserAsync(UserForCreationDto dto)
            => Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = await this.authService.SignUpAsync(dto)
            });


        [HttpPost("send-code")]
        public async Task<IActionResult> SendCodeToPhoneAsync(string phone)
        {

            var result = await this.authService.SendCodeForSignUpAsync(phone);
            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = new { result.Result, result.CachedVerificationMinutes }
            });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyRegisterByPhoneAsync([FromBody] VerifyMessageCodeDto dto)
        {
            var result = await this.authService.VerifySignUpAsync(dto.Phone, dto.Code);
            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = new { result.Result, result.Token }
            });
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(LoginDto dto)
            => Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = await this.authService.AuthenticateAsync(dto.Phone, dto.Password)
            });
    }
}
