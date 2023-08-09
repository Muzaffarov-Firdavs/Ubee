using Microsoft.AspNetCore.Mvc;
using Ubee.Service.DTOs.Logins;
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

        //[HttpPost]
        //[Route("sign-up")]
        //public async Task<IActionResult> PostUserAsync(UserForCreationDto dto)
        //    => Ok(new Response
        //    {
        //        Code = 200,
        //        Message = "Success",
        //        Data = await this.userService.AddUserAsync(dto)
        //    });

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(LoginDto dto)
            => Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = await this.authService.AuthenticateAsync(dto.Username, dto.Password)
            });

        //[HttpPost("send-sms")]
        //public async Task<IActionResult> SendMessageToPhoneAsync(PhoneMessage phoneMessage)
        //    => Ok(new Response
        //    {
        //        Code = 200,
        //        Message = "Success",
        //        Data = await this.phoneService.SendMessageAsync(phoneMessage)
        //    });
    }
}
