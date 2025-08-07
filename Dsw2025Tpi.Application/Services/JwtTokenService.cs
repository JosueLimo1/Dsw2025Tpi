using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Identity;            
using Microsoft.Extensions.Configuration;       
using Microsoft.IdentityModel.Tokens;           
using System.IdentityModel.Tokens.Jwt;          
using System.Security.Claims;                    
using System.Text;                              

namespace Dsw2025Tpi.Application.Services
{
    // Clase concreta que implementa la interfaz IJwtTokenService
    public class JwtTokenService : IJwtTokenService
    {
        // Campo privado que representa la configuración de la aplicación (se obtiene de appsettings.json)
        private readonly IConfiguration _configuration;

        // Constructor que recibe la config de la aplicación por inyección de dependencias
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Método público que genera un token JWT para un usuario de Identity junto a sus roles
        public string GenerateToken(IdentityUser user, IList<string> roles)
        {
            // ======================
            // 1. Obtener valores desde appsettings.json
            // ======================

            var key = _configuration["Jwt:Key"];           // Clave secreta para firmar el token
            var issuer = _configuration["Jwt:Issuer"];     // Identificador del emisor del token (la API)
            var audience = _configuration["Jwt:Audience"]; // Destinatario del token (normalmente el frontend)

            // ======================
            // 2. Validar configuración obligatoria
            // ======================

            // Si falta la clave, se lanza una excepción personalizada controlada
            if (string.IsNullOrWhiteSpace(key))
                throw new BadRequestException("Falta la clave JWT en la configuración");

            // Si falta el issuer, se lanza excepción
            if (string.IsNullOrWhiteSpace(issuer))
                throw new BadRequestException("Falta el issuer JWT en la configuración");

            // Si falta el audience, también se lanza excepción
            if (string.IsNullOrWhiteSpace(audience))
                throw new BadRequestException("Falta el audience JWT en la configuración");

            // ======================
            // 3. Crear los claims del usuario
            // ======================

            // Los claims son datos que se incluyen dentro del token JWT
            var claims = new List<Claim>
            {
                // Claim que representa el ID único del usuario (clave para identificarlo como Customer)
                new Claim(ClaimTypes.NameIdentifier, user.Id),

                // Claim estándar que representa el "sujeto" del token (email en este caso)
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? "noemail"),

                // Claim que representa el nombre del usuario
                new Claim(ClaimTypes.Name, user.UserName ?? "Anonimo"),

                // Identificador único del token (para evitar reutilización)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // ======================
            // 4. Agregar los roles como claims
            // ======================

            // Esto permite utilizar [Authorize(Roles = "...")] en los controladores
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // Claim que representa un rol del usuario
            }

            // ======================
            // 5. Preparar la clave secreta para firmar el token
            // ======================

            // Se codifica la clave en bytes y se usa algoritmo HMAC-SHA256
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Se crean las credenciales de firma con el algoritmo elegido
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // ======================
            // 6. Crear el token JWT con los datos definidos
            // ======================

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,                // Quien emite el token
                audience: audience,            // Quien consume el token
                claims: claims,                // Información del usuario
                expires: DateTime.UtcNow.AddHours(1), // Tiempo de expiración (1 hora desde ahora)
                signingCredentials: credentials // Firma segura
            );

            // ======================
            // 7. Serializar y devolver el token como string , Serializar es convertir un objeto en un texto 
            // ======================

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
