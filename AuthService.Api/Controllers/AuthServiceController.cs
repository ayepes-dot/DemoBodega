using AuthDatabase.Context;
using AuthDomain.AuthDTO;
using AuthDomain.Entities;
using AuthService.Api.AuthCustoms;
using AuthService.Application.Clients;
using DnsClient;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> RevokeToken([FromBody] string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(jti))
                    return BadRequest(new { message = "Token inválido o sin JTI." });

                var existing = await _authServiceDbContext.RevokedTokens
                    .Find(r => r.Jti == jti)
                    .FirstOrDefaultAsync();

                if (existing != null)
                    return BadRequest(new { message = "El token ya fue revocado previamente." });

                var revokedToken = new RevokedToken
                {
                    Jti = jti,
                    UserId = int.Parse(userId ?? "0"),
                    RevokedAt = DateTime.UtcNow,
                    ExpiresAt = jwtToken.ValidTo
                };

                await _authServiceDbContext.RevokedTokens.InsertOneAsync(revokedToken);

                return Ok(new { message = "Token revocado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al revocar el token.", error = ex.Message });
            }
        }

        [HttpPost("validateToken")]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { valid = false, message = "Token vacío o nulo." });

            try
            {
                var raw = token.Trim();
                if (raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    raw = raw.Substring("Bearer ".Length).Trim();

                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken parsed;
                try
                {
                    parsed = handler.ReadJwtToken(raw);
                }
                catch
                {
                    return Unauthorized(new { valid = false, message = "Formato de token inválido." });
                }

                var jti = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                    return Unauthorized(new { valid = false, message = "El token no contiene JTI." });

                var isRevoked = await _authServiceDbContext.RevokedTokens
                    .Find(x => x.Jti == jti)
                    .AnyAsync();

                if (isRevoked)
                    return Unauthorized(new { valid = false, message = "El token ya ha sido revocado." });

                var key = Encoding.UTF8.GetBytes(_config["Jwt:key"]!);
                handler.ValidateToken(raw, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwt = (JwtSecurityToken)validatedToken;
                var userId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                return Ok(new
                {
                    valid = true,
                    message = "Token válido.",
                    userId,
                    email,
                    expira = jwt.ValidTo
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { valid = false, message = "El token ha expirado." });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { valid = false, message = "Token inválido.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { valid = false, message = "Error al validar el token.", error = ex.Message });
            }
        }
    }
}
