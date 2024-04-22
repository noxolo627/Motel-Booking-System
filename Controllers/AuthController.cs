using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotelBooking.Model.DTO;
using MotelBooking.Repository.Interface;
using System.Reflection.Metadata.Ecma335;

namespace MotelBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;

        public AuthController(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            //create the Identity User model
            var user = new IdentityUser
            {
                UserName = request.Email?.Trim(),
                Email = request.Email?.Trim(),
            };

            //create Identity User
            var identityResult = await userManager.CreateAsync(user, request.Password);

            if (identityResult.Succeeded)
            {
                return Ok();
            }
            else
            {
                if(identityResult.Errors.Any())
                {
                    foreach(var error in identityResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return ValidationProblem(ModelState);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            //check email
            var idenityUser = await userManager.FindByEmailAsync(request.Email);

            if(idenityUser is not null)
            {
                //check the password
                var checkPasswordResult = await userManager.CheckPasswordAsync(idenityUser, request.Password);

                if (checkPasswordResult)
                {
                    //get the roles
                    var roles = await userManager.GetRolesAsync(idenityUser);

                    //create a token response
                    var jwtToken = tokenRepository.CreateJwtToken(idenityUser, roles.ToList());

                    var response = new LoginResponseDto
                    {
                        Email = request.Email,
                        Roles = roles.ToList(),
                        Token = jwtToken
                    };

                    return Ok(response);
                }
            }

            var errorMessage = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;
            ModelState.AddModelError("", errorMessage);
            return ValidationProblem(ModelState);
        }
    }
}
