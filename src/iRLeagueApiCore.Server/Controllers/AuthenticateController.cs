using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Authentication;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticateController : Controller
    {
        private readonly ILogger<AuthenticateController> _logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMediator mediator;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            ILogger<AuthenticateController> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMediator mediator,
            IConfiguration configuration)
        {
            _logger = logger;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mediator = mediator;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Log in requested with {UserName}", model.Username);

            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                authClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                _logger.LogInformation("User {UserName} logged in until {ValidTo}", user.UserName, token.ValidTo);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            if (user == null)
            {
                _logger.LogInformation("User {UserName} not found in user database", model.Username);
            }
            else
            {
                _logger.LogInformation("User {UserName} credentials do not match", model.Username);
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("Registering new user {UserName}", model.Username);
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                _logger.LogInformation("User {UserName} already exists", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to add user {UserName} due to errors: {Errors}", model.Username, result.Errors
                    .Select(x => $"{x.Code}: {x.Description}"));
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            _logger.LogInformation("User {UserName} created succesfully", model.Username);
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ResetPassword")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> ResetPassword([FromBody] PasswordResetModel model, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] Generate password reset token for user {User} by {UserName}", "Post", model.UserName, User.Identity?.Name);
            var request = new PasswordResetRequest(model);
            await mediator.Send(request, cancellationToken);
            return NoContent();
        }

        [HttpPost]
        [Route("SetPassword/{userId}")]
        [TypeFilter(typeof(DefaultExceptionFilterAttribute))]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> ResetPasswordWithToken([FromRoute] string userId, [FromBody] SetPasswordTokenModel model, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{Method}] Set Password with reset token for user {UserId}", "Post", userId);
            var request = new SetPasswordWithTokenRequest(userId, model);
            var result = await mediator.Send(request, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Password was set successfully");
                return NoContent();
            }
            _logger.LogInformation("Set password failed");
            return BadRequest();
        }
    }
}
