using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace AuthService.Api.AuthCustoms
{
    public class AuthUtilities
    {
        private readonly IConfiguration _configuration;
        private const int Entropy = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public AuthUtilities(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string jwtGenerator(Usuario usuario)
        {
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UserId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var jwtConfig = new JwtSecurityToken(
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }

        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] entropy = new byte[Entropy];
            Array.Copy(hashBytes, 0, entropy, 0, Entropy);

            var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, entropy, Iterations, HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + Entropy] != hash[i])
                    return false;
            }

            return true;
        }
    }
}
