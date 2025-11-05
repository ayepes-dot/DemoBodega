using System.ComponentModel.DataAnnotations;

namespace InventoryDomain.InventoryEntities
{
    public class DocumentoTransporte
    {
        [Key]
        public int TrasnporDocumentId { get; set; }
        [Required]
        public string Codigo { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente de carga";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public ICollection<Caja> Cajas { get; set; } = new List<Caja>();
    }
}
