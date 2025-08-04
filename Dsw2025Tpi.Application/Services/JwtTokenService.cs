using System.IdentityModel.Tokens.Jwt;          
using System.Security.Claims;                    
using System.Text;                              
using Microsoft.Extensions.Configuration;       
using Microsoft.IdentityModel.Tokens;           
using Microsoft.AspNetCore.Identity;            
using Dsw2025Tpi.Application.Interfaces;        

namespace Dsw2025Tpi.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        // Constructor que recibe IConfiguration para acceder a los valores de configuración del archivo appsettings.json
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Este método genera un token JWT para un usuario de Identity con sus roles incluidos
        public string GenerateToken(IdentityUser user, IList<string> roles)
        {
            // 1. Obtener valores desde el archivo appsettings.json
            var key = _configuration["Jwt:Key"];           // Clave secreta para firmar el token
            var issuer = _configuration["Jwt:Issuer"];     // Quien emite el token (la API)
            var audience = _configuration["Jwt:Audience"]; // Quien consume el token (por ejemplo, un frontend)

            // Validación defensiva por si falta la clave en la configuración
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("Falta la clave JWT en appsettings.json");

            // 2. Crear los claims del usuario
            // Los claims son datos que se incluyen en el token y que representan la identidad del usuario
            var claims = new List<Claim>
            {
                // Sub: sujeto del token (generalmente el email del usuario)
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? "noemail"),

                // Claim personalizado con el nombre del usuario
                new Claim("name", user.UserName ?? "Anonimo"),

                // Jti: identificador único del token (para evitar repeticiones)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 3. Agregar los roles como claims para que el token pueda ser usado con [Authorize(Roles = "...")]
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // ClaimTypes.Role = "role"
            }

            // 4. Preparar la clave secreta para firmar el token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // 5. Configurar el algoritmo de firma (HMAC-SHA256 en este caso)
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 6. Crear el token con todos los datos y configuración
            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,                // Emisor del token
                audience: audience,            // Destinatario del token
                claims: claims,                // Claims definidos arriba
                expires: DateTime.UtcNow.AddHours(1), // Expira en 1 hora
                signingCredentials: credentials // Firma del token
            );

            // 7. Serializar el token y devolverlo como string
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
