using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using W.Ind.Core.Dto;
using W.Ind.Core.Service;
using W.Ind.Core.Helper;
using Sandbox.db.Repositories;

namespace Sandbox.api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    // Injected core services
    protected readonly IUserService _userService;
    protected readonly IJwtService _jwtService;

    // Injected user-defined repository
    protected readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthController(IUserService userService, IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository)
    {
        _userService = userService;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<IdentityResult>> RegisterAsync(UserRegistration dto) 
    {
        try
        {
            // Returns the result from UserManager.RegisterAsync
            IdentityResult result = await _userService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (Exception) 
        { throw; }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest dto) 
    {
        try
        {
            // Validates login request and includes bearer token on success
            LoginResponse response = await _userService.ValidateLoginAsync(dto);


            // Attach refresh token on success
            ShouldAttachRefreshToken(response);

            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            // Token was invalid
            return NoTokenResponse();
        }
        catch (Exception) { throw; }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync() 
    {
        try
        {
            // .InvalidateToken(token): Jwt Service method that invalidates an access token
            // .GetAccessToken(): W.Ind.Core extension helper method to get current bearer token from context
            _jwtService.InvalidateToken(HttpContext.GetAccessToken());


            // Remove response bearer token
            return NoTokenResponse(false);
        }
        catch (Exception) { throw; }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> RefreshTokenAsync([FromBody] string token) 
    {
        try
        {
            // RefreshAsync(token): Invalidates passed refresh token and returns a newly generated token
            var result = await _refreshTokenRepository.RefreshAsync(token);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            // Token was invalid
            return NoTokenResponse();
        }
        catch (Exception) { throw; }
    }

    // private method: Attaches refresh token to login response if login was successful
    private void ShouldAttachRefreshToken(LoginResponse response)
    {
        if (response.Success)
        {
            var refreshTokenResponse = _refreshTokenRepository.Generate(response.Tokens.First().Token);
            response.Tokens.Add(refreshTokenResponse);
        }
    }

    // private method: Removes bearer token from response and returns the appropriate ActionResult
    private ActionResult NoTokenResponse(bool unauthorized = true) 
    {
        HttpContext.Response.Headers["Authorization"] = String.Empty;

        return unauthorized ? Unauthorized() : Ok();
    }
}
