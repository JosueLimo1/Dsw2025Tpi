using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Dsw2025Tpi.Application.Validation;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly Dsw2025TpiContext _dbContext;

    public AuthenticateController(
        UserManager<IdentityUser> userManager,
        IJwtTokenService jwtTokenService,
        Dsw2025TpiContext dbContext)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest("Datos inválidos.");

        if (string.IsNullOrWhiteSpace(model.Email))
            return BadRequest("El email es obligatorio.");

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            return BadRequest("Ya existe un usuario con ese email.");

        var newUser = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(newUser, model.Password);

        if (!result.Succeeded)
        {
            var errores = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest("Error al crear el usuario: " + errores);
        }

        await _userManager.AddToRoleAsync(newUser, "User");

        var customer = new Customer
        {
            Id = Guid.Parse(newUser.Id),
            Name = model.Name,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };

        CustomerValidator.Validate(customer);
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        return Ok("Usuario registrado correctamente con rol 'User'.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
            return Unauthorized("Usuario no encontrado.");

        var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isValid)
            return Unauthorized("Contraseña incorrecta.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user, roles);

        return Ok(new { token });
    }
}