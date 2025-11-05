using System.ComponentModel.DataAnnotations;

namespace InventoryDomain.InventoryEntities
{
    public class Caja
    {
        [Key]
        public int CajaId { get; set; }
        [Required]
        public string CodigoCaja { get; set; } = string.Empty;
        public int DocumentoTransporteId { get; set; }
        public int CantidadReferencias { get; set; }
        public int CantidadBackorder { get; set; }
        public string Estado { get; set; } = "Sin Desempacar";
        public DateTime FechaInsercion { get; set; } = DateTime.Now;

        public DocumentoTransporte DocumentoTransporte { get; set; }
        public ICollection<Referencia> Referencias { get; set; } = new List<Referencia>();
    }
}
