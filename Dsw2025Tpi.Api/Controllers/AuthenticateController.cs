using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Dsw2025Tpi.Application.Validation;

// Define el namespace donde se ubica este controlador
namespace Dsw2025Tpi.Api.Controllers;

// Marca esta clase como un controlador de API que usa validaciones automáticas del modelo
[ApiController]

// Define la ruta base del controlador: /api/authenticate
[Route("api/[controller]")]

// Permite que todos los endpoints de este controlador sean accesibles sin autenticación previa (sin token JWT)
[AllowAnonymous]
public class AuthenticateController : ControllerBase // Hereda de ControllerBase, que es la clase base para APIs REST
{
    // Inyección de dependencias para gestionar usuarios de Identity
    // Herramienta para crear, buscar y manejar usuarios
    private readonly UserManager<IdentityUser> _userManager;

    // Servicio personalizado para generar tokens JWT
    private readonly IJwtTokenService _jwtTokenService;

    // Contexto de base de datos propio del sistema (para acceder a Customers, etc.)
    private readonly Dsw2025TpiContext _dbContext;

    // Constructor que recibe las dependencias necesarias por inyección
    public AuthenticateController(
        UserManager<IdentityUser> userManager,       // Servicio de gestión de usuarios
        IJwtTokenService jwtTokenService,            // Servicio para crear JWT
        Dsw2025TpiContext dbContext)                 // Contexto de datos
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
    }

    // ======================================================
    // POST /api/authenticate/register
    // Registra un nuevo usuario con Identity y también como Customer en tu sistema
    // ======================================================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model) // Recibe los datos de registro en el cuerpo de la petición
    {
        // Valida el modelo recibido según las anotaciones (por ejemplo, campos obligatorios)
        if (!ModelState.IsValid)
            return BadRequest("Datos inválidos."); // Si hay errores, devuelve 400 Bad Request

        // Verifica que el mail de usuario no esté vacío
        if (string.IsNullOrWhiteSpace(model.Email))
            return BadRequest("El email es obligatorio.");

        // Verifica si ya existe un usuario con el mismo email
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            return BadRequest("Ya existe un usuario con ese email.");

        // Crea un nuevo usuario de Identity con los datos básicos
        var newUser = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = true // Se marca como confirmado para evitar paso de validación por email
        };

        // Intenta registrar el usuario en la base de datos de Identity usando la contraseña proporcionada
        var result = await _userManager.CreateAsync(newUser, model.Password);

        // Si falló el registro, devuelve los errores como mensaje
        if (!result.Succeeded)
        {
            var errores = string.Join("; ", result.Errors.Select(e => e.Description)); // Une todos los mensajes de error
            return BadRequest("Error al crear el usuario: " + errores);
        }

        // Asigna el rol "User" al nuevo usuario
        await _userManager.AddToRoleAsync(newUser, "User");

        // Crea un registro en la tabla Customer del sistema, con el mismo ID que el IdentityUser
        var customer = new Customer
        {
            Id = Guid.Parse(newUser.Id), // Relaciona IdentityUser con Customer mediante el mismo GUID
            Name = model.Name,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };

        // ✅ Validamos el objeto customer usando el validador personalizado
        CustomerValidator.Validate(customer);

        // Agrega el nuevo Customer al contexto
        _dbContext.Customers.Add(customer);

        // Guarda los cambios en la base de datos
        await _dbContext.SaveChangesAsync();

        // Devuelve una respuesta exitosa
        return Ok("Usuario registrado correctamente con rol 'User'.");
    }

    // ======================================================
    // POST /api/authenticate/login
    // Verifica las credenciales del usuario y devuelve un JWT si son válidas
    // ======================================================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model) // Recibe nombre de usuario y contraseña
    {
        // Busca al usuario por nombre de usuario
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
            return Unauthorized("Usuario no encontrado."); // Devuelve 401 si no existe

        // Verifica que la contraseña sea correcta
        var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isValid)
            return Unauthorized("Contraseña incorrecta."); // Devuelve 401 si la contraseña no coincide

        // Obtiene los roles asociados al usuario (por ejemplo, "User", "Admin")
        var roles = await _userManager.GetRolesAsync(user);

        // Genera el token JWT con la información del usuario y sus roles
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Devuelve el token al frontend dentro de un objeto anónimo
        return Ok(new { token });
    }
}
