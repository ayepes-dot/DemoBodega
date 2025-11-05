using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthDomain.AuthDTO
{
    public class UserLoginDTO
    {
        public int UserId { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty; 
    }
}
