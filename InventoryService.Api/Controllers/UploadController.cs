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

        public UploadController(ExcelProcessingService excelService)
        {
            _excelService = excelService;
        }

        [HttpPost("upload-excel")]
        [Authorize]
        public async Task<IActionResult> UploadExcel([FromQuery] int documentoId, IFormFile file)
        {
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
