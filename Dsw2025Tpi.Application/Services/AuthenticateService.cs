using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validation;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Application.Services
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenService _jwtService;
        private readonly Dsw2025TpiContext _db;

        public AuthenticateService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenService jwtService,
            Dsw2025TpiContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _db = db;
        }

        // ================================================
        // LOGIN
        // ================================================
        public async Task<string> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
                throw new BadRequestException("Usuario no encontrado.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                throw new BadRequestException("Contraseña incorrecta.");

            var roles = await _userManager.GetRolesAsync(user);
            return _jwtService.GenerateToken(user, roles);
        }

        // ================================================
        // REGISTRO DESDE MODAL (ROLE = USER)
        // ================================================
        public async Task<string> ClientRegisterAsync(ClientRegisterModel model)
        {
            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                throw new BadRequestException("No se pudo crear el usuario.");

            // rol user
            await _userManager.AddToRoleAsync(user, "User");

            var customer = new Customer
            {
                Id = Guid.Parse(user.Id),
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            CustomerValidator.Validate(customer);

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return "Cliente registrado correctamente.";
        }

        // ================================================
        // REGISTRO (ADMIN CREA ADMIN O USER)
        // ================================================
        public async Task<string> RegisterAsync(RegisterModel model)
        {
            if (model.Role != "Admin" && model.Role != "User")
                throw new BadRequestException("El rol debe ser Admin o User.");

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                throw new BadRequestException("No se pudo crear el usuario.");

            await _userManager.AddToRoleAsync(user, model.Role);

            var customer = new Customer
            {
                Id = Guid.Parse(user.Id),
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            CustomerValidator.Validate(customer);

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return $"Usuario creado correctamente con rol {model.Role}.";
        }
    }
}
