using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryDomain.InventoryDTO
{
    public class TokenValidationResponseDTO
    {
        public bool Valid { get; set; }
        public int userId { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
    }
}