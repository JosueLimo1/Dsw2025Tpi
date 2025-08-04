using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // Ruta base: /api/authenticate
[AllowAnonymous] // Todos los endpoints pueden ser accedidos sin token
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

    // ===============================
    // POST /api/authenticate/register
    // Registra un nuevo usuario con rol "User" y lo guarda también como Customer
    // ===============================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // Validación básica
        if (!ModelState.IsValid)
            return BadRequest("Datos inválidos.");

        // Verificar si ya existe un usuario con ese email
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            return BadRequest("Ya existe un usuario con ese email.");

        // Crear nuevo usuario para Identity
        var newUser = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = true
        };

        // Intentar guardar el usuario en la base de datos de autenticación
        var result = await _userManager.CreateAsync(newUser, model.Password);
        if (!result.Succeeded)
        {
            var errores = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest("Error al crear el usuario: " + errores);
        }

        // Asignar rol "User"
        await _userManager.AddToRoleAsync(newUser, "User");

        // Crear también una entrada en la tabla Customer con el mismo ID del usuario
        var customer = new Customer
        {
            Id = Guid.Parse(newUser.Id), // Relación directa entre Identity y Customer
            Name = model.Name,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        return Ok("Usuario registrado correctamente con rol 'User'.");
    }

    // ===============================
    // POST /api/authenticate/login
    // Valida credenciales y devuelve un token JWT válido
    // ===============================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // Buscar usuario por nombre
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
            return Unauthorized("Usuario no encontrado.");

        // Validar contraseña
        var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isValid)
            return Unauthorized("Contraseña incorrecta.");

        // Obtener roles del usuario
        var roles = await _userManager.GetRolesAsync(user);

        // Generar token con datos y roles incluidos
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Retornar token al frontend
        return Ok(new { token });
    }
}