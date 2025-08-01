using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class JwtTokenService
    {
        // Campo privado para acceder a la configuración del sistema (appsettings.json)
        private readonly IConfiguration _config;

        // Constructor que recibe la configuración por inyección de dependencias
        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        // Método que genera un token JWT a partir del nombre de usuario
        public string GenerateToken(string username)
        {
            // Obtiene la sección "Jwt" del archivo de configuración
            var jwtConfig = _config.GetSection("Jwt");

            // Extrae la clave secreta desde la configuración. Si no existe, lanza una excepción
            var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt Key");

            // Convierte la clave secreta en una clave simétrica que usará el token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));

            // Define las credenciales de firma usando el algoritmo HMAC SHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define los "claims", es decir, los datos que viajarán dentro del token
            var claims = new[]
            {
            // Claim estándar que representa el sujeto del token (el usuario)
            new Claim(JwtRegisteredClaimNames.Sub, username),

            // Claim estándar que representa un identificador único para el token (útil para revocarlo)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // Acá podrías agregar más claims, como el rol del usuario (ej: new Claim("role", role))
        };

            // Crea el token JWT con todos los datos: emisor, audiencia, claims, tiempo de expiración y firma
            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"], // Quién emite el token (por ejemplo, tu API)
                audience: jwtConfig["Audience"], // Quién consume el token (por ejemplo, el frontend)
                claims: claims, // Los datos que viajan dentro del token
                expires: DateTime.Now.AddMinutes(double.Parse(jwtConfig["ExpireInMinutes"] ?? "60")), // Tiempo de validez
                signingCredentials: creds // Firma digital que garantiza que el token no fue modificado
            );

            // Convierte el token en un string (formato JWT) para enviarlo al cliente
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
