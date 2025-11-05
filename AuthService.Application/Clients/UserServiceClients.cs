using AuthDomain.AuthDTO;
using Domain.DTO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace AuthService.Application.Clients
{
    public class UserServiceClients
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public UserServiceClients(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("UserServiceClients");
            _config = config;
        }

        public async Task<UserLoginDTO?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email no puede ser nulo o vacío.");

            string endpoint = "api/UserService/GetUserForAuth";

            var response = await _httpClient.PostAsJsonAsync(endpoint, new { Correo = email });

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al contactar UserService ({response.StatusCode})");

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<UserLoginDTO>(json);
        }
    }
}
