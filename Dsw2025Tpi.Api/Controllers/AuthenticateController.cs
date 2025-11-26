using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{
    private readonly IAuthenticateService _authService;

    public AuthenticateController(IAuthenticateService authService)
    {
        _authService = authService;
    }

    // LOGIN
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var token = await _authService.LoginAsync(model);
        return Ok(new { token });
    }

    // REGISTRO CLIENTE (modal)
    [HttpPost("client-register")]
    [AllowAnonymous]
    public async Task<IActionResult> ClientRegister([FromBody] ClientRegisterModel model)
    {
        var result = await _authService.ClientRegisterAsync(model);
        return Ok(result);
    }

    // REGISTRO ADMIN (elige rol)
    [HttpPost("admin-register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var result = await _authService.RegisterAsync(model);
        return Ok(result);
    }
}

