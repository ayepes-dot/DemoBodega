using ExcelDataReader;
using InventoryDatabase.Context;
using InventoryDomain.InventoryEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;
using System.Text;

namespace InventoryServices.Services
{
    public class ExcelProcessingService
    {
        private readonly InventoryDbContext _context;

        public ExcelProcessingService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task ProcesarExcelAsync(IFormFile file, int documentoId)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Debe subir un archivo Excel válido.");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var result = reader.AsDataSet();
            if (result.Tables.Count == 0)
                throw new Exception("El archivo Excel no contiene hojas válidas.");

            var table = result.Tables[0];
            if (table.Rows.Count < 2)
                throw new Exception("El archivo no contiene registros válidos.");

            var documento = await _context.DocumentosTransporte
                .Include(d => d.Cajas)
                .ThenInclude(c => c.Referencias)
                .FirstOrDefaultAsync(d => d.TrasnporDocumentId == documentoId);

            if (documento == null)
                throw new Exception("Documento de transporte no encontrado.");

            var cajasMap = new Dictionary<string, Caja>();


            for (int i = 1; i < table.Rows.Count; i++)
            {
                string codigoCaja = table.Rows[i][0]?.ToString()?.Trim() ?? "";
                string codigoRef = table.Rows[i][1]?.ToString()?.Trim() ?? "";
                string nombreRef = table.Rows[i][2]?.ToString()?.Trim() ?? "";
                string backorderValue = table.Rows[i][3]?.ToString()?.Trim()?.ToLower() ?? "";

                bool tieneBackorder = backorderValue == "true" || backorderValue == "1" || backorderValue == "sí" || backorderValue == "si";

                if (string.IsNullOrEmpty(codigoCaja) || string.IsNullOrEmpty(codigoRef))
                    continue;

                if (!cajasMap.ContainsKey(codigoCaja))
                {
                    var nuevaCaja = new Caja
                    {
                        CodigoCaja = codigoCaja,
                        DocumentoTransporteId = documentoId,
                        Estado = "Sin Desempacar",
                        FechaInsercion = DateTime.Now
                    };
                    _context.Cajas.Add(nuevaCaja);
                    cajasMap[codigoCaja] = nuevaCaja;
                }

                var referencia = new Referencia
                {
                    CodigoReferencia = codigoRef,
                    NombreReferencia = nombreRef,
                    TieneBackorder = tieneBackorder,
                    Estado = "Sin Desempacar",
                    Caja = cajasMap[codigoCaja]
                };

                _context.Referencias.Add(referencia);
            }

            await _context.SaveChangesAsync();

            foreach (var caja in cajasMap.Values)
            {
                int cantidadRefs = await _context.Referencias.CountAsync(r => r.CajaId == caja.CajaId);
                int cantidadBackorder = await _context.Referencias.CountAsync(r => r.CajaId == caja.CajaId && r.TieneBackorder);

                caja.CantidadReferencias = cantidadRefs;
                caja.CantidadBackorder = cantidadBackorder;

                if (cantidadBackorder > 0)
                    caja.Estado = "Backorder";
            }

            documento.Estado = "Procesado";

            await _context.SaveChangesAsync();
        }
    }
}
