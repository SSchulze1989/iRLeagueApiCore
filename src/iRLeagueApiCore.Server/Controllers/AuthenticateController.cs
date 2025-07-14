using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Common.Responses;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Authentication;
using iRLeagueApiCore.Server.Handlers.Users;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace iRLeagueApiCore.Server.Controllers;

[ApiController]
[TypeFilter(typeof(DefaultExceptionFilterAttribute))]
[Route("[controller]")]
public sealed class AuthenticateController : Controller
{
    private readonly ILogger<AuthenticateController> _logger;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly IMediator mediator;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationSection jwtConfig;

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
        jwtConfig = _configuration.GetRequiredSection("JWT");
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        _logger.LogInformation("Log in requested with {UserName}", model.Username);
        var userAgent = Request.Headers.UserAgent.ToString();

        // Get user by Username or Emailadress
        var user = await userManager.FindByNameAsync(model.Username) 
            ?? await userManager.FindByEmailAsync(model.Username);

        if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
        {
            if (user.EmailConfirmed == false)
            {
                return Unauthorized(new UnauthorizedResponse()
                {
                    Status = "MailConfirm",
                    Errors = new object[] { "Missing email confirmation" },
                });
            }

            var idToken = await CreateIdToken(user, userAgent);
            if (idToken is null)
            {
                return Unauthorized(new UnauthorizedResponse()
                {
                    Status = "Login failed",
                    Errors = new object[] { "Could not generate id token" },
                });
            }
            var accessToken = await CreateAccessTokenAsync(user);
            if (accessToken is null)
            {
                return Unauthorized(new UnauthorizedResponse()
                {
                    Status = "Login failed",
                    Errors = new object[] { "Could not generate access token" },
                });
            }

            _logger.LogInformation("User {UserName} logged in until {ValidTo}", user.UserName, idToken.ValidTo);
            var tokenHandler = new JwtSecurityTokenHandler();
            return Ok(new
            {
                idToken = tokenHandler.WriteToken(idToken),
                accessToken = tokenHandler.WriteToken(accessToken),
                expiration = idToken.ValidTo
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

        return Unauthorized(new UnauthorizedResponse()
        {
            Status = "Wrong username or password",
            Errors = Array.Empty<object>(),
        });
    }

    [HttpPost]
    [Route("authorize")]
    public async Task<IActionResult> Authorize([FromBody] AuthorizeModel model)
    {
        _logger.LogInformation("Requesting access by idToken");

        // decrypt token
        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = jwtConfig["Secret"] ?? throw new InvalidConfigurationException("Missing JWT secret");
        var validationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = jwtConfig["ValidAudience"],
            ValidIssuer = jwtConfig["ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        };

        try
        {
            var principal = tokenHandler.ValidateToken(model.IdToken, validationParameters, out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            var loginKey = jwtToken.Claims.FirstOrDefault(x => x.Type == "idKey")?.Value;
            if (loginKey is null)
            {
                return Unauthorized();
            }
            var user = await userManager.FindByLoginAsync("iRLeagueApiCore", loginKey);
            if (user is null)
            {
                return Unauthorized();
            }

            var accessToken = await CreateAccessTokenAsync(user);
            if (accessToken is null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("User {UserName} granted access until {ValidTo}", user.UserName, accessToken.ValidTo);
            return Ok(new
            {
                accessToken = tokenHandler.WriteToken(accessToken),
                expiration = accessToken.ValidTo
            });
        }
        catch (SecurityTokenValidationException)
        {
            _logger.LogWarning("Invalid token provided");
            return Unauthorized();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is SecurityTokenException || ex is InvalidCastException)
        {
            return Unauthorized();
        }
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model, [FromQuery] string? linkTemplate = null)
    {
        if (string.IsNullOrWhiteSpace(linkTemplate))
        {
            var baseUri = $"https://{Request.Host.Value}";
            linkTemplate = $$"""{{baseUri}}/users/{userId}/confirm/{token}""";
        }
        var request = new RegisterUserRequest(model, linkTemplate);
        var (user, result) = await mediator.Send(request);
        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(UsersController.GetUser), "Users", new { id = user.UserId }, user);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("ResetPassword")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult> ResetPassword([FromBody] PasswordResetModel model, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{Method}] Generate password reset token using {Email} by {UserName}", "Post", model.Email, User.Identity?.Name);
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

    private async Task<JwtSecurityToken?> CreateIdToken(ApplicationUser user, string userAgent)
    {
        // prune expired logins
        var logins = await userManager.GetLoginsAsync(user);
        foreach (var login in logins)
        {
            var (id, exp) = DecodeLoginKey(login.ProviderKey);
            if (exp <= DateTime.UtcNow)
            {
                await userManager.RemoveLoginAsync(user, "iRLeagueApiCore", login.ProviderKey);
            }
        }

        var expiration = DateTime.UtcNow.AddMonths(3);
        var loginKey = GenerateLoginkey(expiration);
        var loginInfo = new UserLoginInfo("iRLeagueApiCore", loginKey, "iRLeagueApiCore");
        var result = await userManager.AddLoginAsync(user, loginInfo);
        if (result.Succeeded == false)
        {
            return null;
        }
        var idClaims = new List<Claim>()
        {
            new Claim("idKey", loginKey),
        };

        var secret = jwtConfig["Secret"] ?? throw new InvalidConfigurationException("Missing JWT secret");
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: expiration,
            claims: idClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

        return token;
    }

    private static string GenerateLoginkey(DateTime expiration)
    {
        // encode generated id and browser information as base64 string
        var guid = Guid.NewGuid();
        var keyString = string.Join('&', guid, expiration.ToString(CultureInfo.InvariantCulture));
        var bytes = Encoding.UTF8.GetBytes(keyString);
        return Convert.ToBase64String(bytes);
    }

    private (string id, DateTime expiration) DecodeLoginKey(string loginKey)
    {
        var bytes = Convert.FromBase64String(loginKey);
        var keyString = Encoding.UTF8.GetString(bytes);

        var parts = keyString.Split('&');
        var id = parts.ElementAtOrDefault(0) ?? throw new InvalidOperationException();
        if (DateTime.TryParse(parts.ElementAtOrDefault(1), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime expiration) == false)
        {
            throw new InvalidOperationException();
        }
        return (id, expiration);
    }

    private async Task<JwtSecurityToken?> CreateAccessTokenAsync(ApplicationUser user)
    {
        var userRoles = await userManager.GetRolesAsync(user);

        var userName = user.UserName ?? throw new InvalidOperationException("ApplicationUser.UserName may not be null");
        var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        authClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

        var secret = jwtConfig["Secret"] ?? throw new InvalidConfigurationException("Missing JWT secret");
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddMinutes(10),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

        return token;
    }
}
