using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación para todos los endpoints salvo que se indique lo contrario
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticateController(UserManager<IdentityUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    // Endpoint para registrar nuevos usuarios con rol "User" por defecto.
    // Solo accesible para usuarios con rol "Admin"
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // Verifica si ya existe un usuario con el mismo email
        var existingUser = await _userManager.FindByEmailAsync(model.eMail);
        if (existingUser != null)
            return BadRequest("Ya existe un usuario con ese email.");

        // Crea un nuevo usuario de Identity
        var newUser = new IdentityUser
        {
            UserName = model.Username,
            Email = model.eMail,
            EmailConfirmed = true
        };

        // Intenta guardar el nuevo usuario con la contraseña proporcionada
        var result = await _userManager.CreateAsync(newUser, model.Password);

        if (!result.Succeeded)
            return BadRequest("Error al crear el usuario: " + string.Join("; ", result.Errors.Select(e => e.Description)));

        // Asigna el rol "User" por defecto
        await _userManager.AddToRoleAsync(newUser, "User");

        return Ok("Usuario registrado correctamente con rol 'User'.");
    }

    // Endpoint para login de usuario. Acceso público.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // Busca el usuario por nombre
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
            return Unauthorized("Usuario no encontrado.");

        // Verifica la contraseña
        var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isValid)
            return Unauthorized("Contraseña incorrecta.");

        // Obtiene los roles del usuario para incluirlos en el token
        var roles = await _userManager.GetRolesAsync(user);

        // Genera el token JWT con los claims necesarios
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Retorna el token como respuesta
        return Ok(new { token });
    }
}

