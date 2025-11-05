using AuthDatabase.Context;
using AuthDomain.AuthDTO;
using AuthDomain.Entities;
using AuthService.Api.AuthCustoms;
using AuthService.Application.Clients;
using DnsClient;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AuthService.Api.Controllers
{
    public class AuthServiceController : Controller
    {
        private readonly UserServiceClients _userService;
        private readonly AuthServiceDbContext _authServiceDbContext;
        private readonly AuthUtilities _utils;
        private readonly IConfiguration _config;

        public AuthServiceController(AuthServiceDbContext authServiceDb, AuthUtilities utils, IConfiguration config, UserServiceClients userService)
        {
            _userService = userService;
            _authServiceDbContext = authServiceDb;
            _utils = utils;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
                    return BadRequest(new { message = "Credenciales inválidas." });

                var user = await _userService.GetUserByEmailAsync(login.Email);
                if (user == null)
                    return Unauthorized(new { message = "Usuario no encontrado." });

                bool isValid = _utils.VerifyPassword(login.Password, user.Clave);
                if (!isValid)
                    return Unauthorized(new { message = "Contraseña incorrecta." });

                var token = _utils.jwtGenerator(new Usuario
                {
                    UserId = user.UserId,
                    Correo = user.Correo
                });

                return Ok(new
                {
                    message = "Inicio de sesión exitoso.",
                    token
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new { message = "El servicio de usuarios no está disponible." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor.", error = ex.Message });
            }
        }

        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] string jti)
        {
            var token = new RevokedToken
            {
                Jti = jti,
                UserId = 1,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_config["Jwt:ExpiresMinutes"]))
            };

            await _authServiceDbContext.RevokedTokens.InsertOneAsync(token);
            return Ok(new
            {
                message = "Token revocado correctamente.",
                token.Jti,
                token.RevokedAt
            });
        }
    }
}
