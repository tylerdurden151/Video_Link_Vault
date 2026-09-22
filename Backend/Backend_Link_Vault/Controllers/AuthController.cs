using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend_Link_Vault.Models;
using Backend_Link_Vault.DTO;
using Backend_Link_Vault.Services;
using Backend_Link_Vault.Interfaces;

namespace Backend_Link_Vault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserStore _userStore;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IVideoLinkRepository _videoLinkRepository;

    private const string DemoAccountEmail = "timothy@example.com";

    public AuthController(UserStore userStore, IPasswordHasher<User> passwordHasher, IVideoLinkRepository videoLinkRepository)
    {
        _userStore = userStore;
        _passwordHasher = passwordHasher;
        _videoLinkRepository = videoLinkRepository;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        if (await _userStore.FindByEmailAsync(request.Email) is not null)
        {
            return Conflict("An account with that email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.PasswordHash);

        await _userStore.AddAsync(user);

        if (string.Equals(user.Email, DemoAccountEmail, StringComparison.OrdinalIgnoreCase))
        {
            await _videoLinkRepository.SeedDemoDataAsync(user.Id);
        }
        return Ok(ToResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
    {
        var user = await _userStore.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.PasswordHash);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(ToResponse(user));
    }

    private static UserResponse ToResponse(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
    };
}
