using Microsoft.AspNetCore.Mvc;
using Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Database.Context;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Domain.DTO;
using Demo.Customs;


namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserServiceController : Controller
    {
        private readonly UserServiceDbContext _userServiceDbContext;
        private readonly Utilities _utils;

        public UserServiceController(UserServiceDbContext userServiceDb, Utilities utilities)
        {
            _userServiceDbContext = userServiceDb;
            _utils = utilities;
        }

        [HttpPost("SingUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDTO) 
        {
            if (signUpDTO == null)
                return BadRequest("Datos invalidos");

            var usuario = new Usuario()
            {
                Nombre = signUpDTO.Nombre,
                Correo = signUpDTO.Email,
                Clave = _utils.hashPass(signUpDTO.Clave)
            };

            var existeUsuario = await _userServiceDbContext.Usuarios
                .AnyAsync(u => u.Correo == signUpDTO.Email);

            if (existeUsuario)
                return BadRequest("El correo " + signUpDTO.Email + " ya se encuentra registrado.");

            await _userServiceDbContext.Usuarios.AddAsync(usuario);
            await _userServiceDbContext.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario creado con éxito", usuario.UserId });
        }

        [HttpPost("GetAllUsers")]
        public async Task<IEnumerable<UserSearchDTO>> GetAllUsers([FromBody] UserSearchDTO userSearchDTO)
        {
            var query = _userServiceDbContext.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userSearchDTO.Nombre))
                query = query.Where(u => EF.Functions.Like(u.Nombre, $"{userSearchDTO.Nombre}%"));

            return await query.Select(u => new UserSearchDTO
            {
                Nombre = u.Nombre,
                Correo = u.Correo
            }).ToListAsync();
        }

        [HttpPut("UpdateUsers/{userId}")]
        public async Task<bool> UpdateUser(int userId, [FromBody]UpdateUsersDTO userUpdateDTO)
        {
            var user = await _userServiceDbContext.Usuarios.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return false;

            if(!string.IsNullOrWhiteSpace(userUpdateDTO.Nombre))
                user.Nombre = userUpdateDTO.Nombre;
            if (!string.IsNullOrWhiteSpace(userUpdateDTO.Correo))
                user.Correo = userUpdateDTO.Correo;

            _userServiceDbContext.Usuarios.Update(user);
            await _userServiceDbContext.SaveChangesAsync();
            return true;
        }

        [HttpDelete("DeleteUsers/{userId}")]
        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _userServiceDbContext.Usuarios.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return false;

            _userServiceDbContext.Usuarios.Remove(user);
            await _userServiceDbContext.SaveChangesAsync();
            return true;
        }

        [HttpPost("GetUserForAuth")]
        public async Task<IActionResult> GetUserForAuth([FromBody] EmailRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo))
                return BadRequest(new { message = "Correo no proporcionado" });

            var user = await _userServiceDbContext.Usuarios
                .Where(u => u.Correo == request.Correo)
                .Select(u => new
                {
                    u.UserId,
                    u.Correo,
                    u.Clave
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(user);
        }
    }
}