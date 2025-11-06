using InventoryDomain.InventoryDTO;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace InventoryServices.Clients
{
    public class AuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("AuthServiceClient");
        }

        public async Task<TokenValidationResponseDTO?> ValidateTokenAsync(string token)
        {
            var response = await _httpClient.PostAsJsonAsync("api/AuthService/validateToken", token);

            if (!response.IsSuccessStatusCode)
                return new TokenValidationResponseDTO
                {
                    Valid = false,
                    Message = $"Error al validar el token: {response.StatusCode}"
                };

            return await response.Content.ReadFromJsonAsync<TokenValidationResponseDTO>();
        }
    }
}
