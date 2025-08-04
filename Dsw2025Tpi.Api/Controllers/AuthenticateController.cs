using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación para todos los endpoints, salvo que se indique lo contrario
public class AuthenticateController : ControllerBase
{
    private readonly Dsw2025TpiContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticateController(Dsw2025TpiContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    // Login de usuario con email y password.
    // Acceso público: no requiere estar autenticado previamente.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // Buscar al cliente por email (caso insensible)
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == model.Username.ToLower());

        if (customer is null)
            return Unauthorized("Usuario no encontrado");

        // Por simplicidad, se valida contra una contraseña fija.
        // En producción debería usarse hashing (Identity).
        if (model.Password != "123456") // Contraseña ficticia
            return Unauthorized("Contraseña incorrecta");

        // Generar token JWT para el cliente
        var token = _jwtTokenService.GenerateToken(customer);

        // Devolver el token
        return Ok(new { token });
    }

    // Registro de un nuevo cliente.
    // Solo los usuarios autenticados con rol "Admin" pueden registrar.
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // Validar que no exista un cliente con el mismo email
        var exists = await _context.Customers.AnyAsync(c => c.Email == model.eMail);
        if (exists)
            return BadRequest("Ya existe un cliente registrado con ese email.");

        // Crear el nuevo cliente
        var customer = new Customer(
            email: model.eMail,
            name: model.Username,
            phoneNumber: null
        )
        {
            Id = Guid.NewGuid()
        };

        // Guardar en la base de datos
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok("Cliente registrado correctamente.");
    }
}

