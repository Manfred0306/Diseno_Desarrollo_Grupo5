using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class ClienteFilaVM
    {
        public int ID_CLIENTE { get; set; }
        public string NOMBRE { get; set; }
        public string TELEFONO { get; set; }
        public string CORREO { get; set; }
        public string DIRECCION { get; set; }
        public int ID_ESTADO { get; set; }
        public string ESTADO { get; set; }
    }

    public class ClienteCrudVM
    {
        // Lista
        public List<ClienteFilaVM> Clientes { get; set; } = new List<ClienteFilaVM>();

        // Para combos
        public List<SelectListItem> Estados { get; set; } = new List<SelectListItem>();

        // Form (Create/Edit)
        public int ID_CLIENTE { get; set; }

        [Required]
        [StringLength(100)]
        public string NOMBRE { get; set; }

        [StringLength(30)]
        public string TELEFONO { get; set; }

        [StringLength(120)]
        [EmailAddress]
        public string CORREO { get; set; }

        [StringLength(200)]
        public string DIRECCION { get; set; }

        public int ID_ESTADO { get; set; } = 1;

        // Búsqueda
        public string Q { get; set; }

        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalItems / PageSize);
    }
}