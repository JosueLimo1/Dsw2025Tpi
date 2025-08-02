using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using DSWIntegral.Models;

namespace Dsw2025Tpi
{
    /// <summary>
    /// Controlador de autenticación: registro y login de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>
        /// Registra un nuevo cliente y retorna token JWT.
        /// </summary>
        /// <param name="dto">Datos de registro: Nombre, Email, Address y Password.</param>
        /// <returns>Token de autenticación y fecha de expiración.</returns>
        /// <response code="200">Registro exitoso.</response>
        /// <response code="400">Bad Request. Email ya existe.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
       [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _auth.RegisterAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Email duplicado
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Error inesperado
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Inicia sesión con credenciales válidas y retorna token JWT.
        /// </summary>
        /// <param name="dto">Credenciales de login: Email y Password.</param>
        /// <returns>Token de autenticación y fecha de expiración.</returns>
        /// <response code="200">Login exitoso.</response>
        /// <response code="400">Bad Request. Credenciales inválidas.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _auth.LoginAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                // Credenciales inválidas
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Error inesperado
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }
    }
}