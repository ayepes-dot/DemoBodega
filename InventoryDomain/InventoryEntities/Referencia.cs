using System.ComponentModel.DataAnnotations;

namespace InventoryDomain.InventoryEntities
{
    public class Referencia
    {
        [Key]
        public int ReferenciaId { get; set; }
        public int CajaId { get; set; }
        [Required]
        public string CodigoReferencia { get; set; } = string.Empty;
        public string NombreReferencia { get; set; } = string.Empty;
        public bool TieneBackorder { get; set; }
        public string Estado { get; set; } = "Sin Desempacar";

        public Caja Caja { get; set; }
    }
}
