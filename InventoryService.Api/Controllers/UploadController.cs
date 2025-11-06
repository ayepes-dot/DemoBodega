using InventoryServices.Clients;
using InventoryServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        private readonly ExcelProcessingService _excelService;
        private readonly AuthServiceClient _authClient;

        public UploadController(ExcelProcessingService excelService, AuthServiceClient authClient)
        {
            _excelService = excelService;
            _authClient = authClient;
        }

        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel([FromQuery] int documentoId, IFormFile file)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
                return Unauthorized(new { message = "Falta el token JWT en el encabezado." });

            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader.Substring("Bearer ".Length).Trim()
                : authHeader.Trim();

            var validation = await _authClient.ValidateTokenAsync(token);
            if (validation == null || !validation.Valid)
                return Unauthorized(new { message = "Token inválido o expirado.", error = validation?.Message });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Debe subir un archivo Excel válido." });

            try
            {
                await _excelService.ProcesarExcelAsync(file, documentoId);
                return Ok(new { message = "Archivo procesado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al procesar el archivo.", error = ex.Message });
            }
        }
    }
}
