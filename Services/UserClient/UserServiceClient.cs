using Domain.DTO;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace AuthClientServices.UserClient
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public UserServiceClient(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("UserServiceClient");
            _config = config;
        }

        public async Task<UserSearchDTO?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email no puede ser nulo o vacío.");

            // Endpoint del UserService
            string endpoint = "api/UserService/GetAllUsers";

            var response = await _httpClient.PostAsJsonAsync(endpoint, new { Nombre = "", Correo = email });

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al contactar UserService ({response.StatusCode})");

            var json = await response.Content.ReadAsStringAsync();
            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserSearchDTO>>(json);

            return users?.FirstOrDefault(u => u.Correo == email);
        }
    }
}
