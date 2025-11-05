using Microsoft.AspNetCore.Mvc;
using SecurityDomain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecurityService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityServiceController : Controller
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public SecurityServiceController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient("AuthServiceClient");
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] string token)
        {
            var result = new TokenValidationResult();

            try
            {
                // 1️⃣ Verificar estructura y firma JWT
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var principal = handler.ValidateToken(token, parameters, out var validatedToken);
                var jwt = (JwtSecurityToken)validatedToken;
                var jti = jwt.Id;

                // 2️⃣ Consultar AuthService si el token está revocado
                var response = await _httpClient.GetAsync($"api/AuthService/validateToken/{jti}");
                if (!response.IsSuccessStatusCode)
                {
                    result.Valid = false;
                    result.Message = "No se pudo contactar con el AuthService.";
                    return Ok(result);
                }

                var validationResponse = await response.Content.ReadFromJsonAsync<dynamic>();
                bool valid = validationResponse.valid ?? false;

                if (!valid)
                {
                    result.Valid = false;
                    result.Message = validationResponse.message ?? "Token revocado.";
                    return Ok(result);
                }

                // 3️⃣ Extraer información del usuario
                result.Valid = true;
                result.UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                result.Email = principal.FindFirst(ClaimTypes.Email)?.Value;
                result.Role = principal.FindFirst(ClaimTypes.Role)?.Value;
                result.Message = "Token válido.";

                return Ok(result);
            }
            catch (SecurityTokenExpiredException)
            {
                result.Valid = false;
                result.Message = "El token ha expirado.";
            }
            catch (SecurityTokenException)
            {
                result.Valid = false;
                result.Message = "Token inválido.";
            }
            catch (Exception ex)
            {
                result.Valid = false;
                result.Message = $"Error interno: {ex.Message}";
            }

            return Ok(result);
        }
    }
}
