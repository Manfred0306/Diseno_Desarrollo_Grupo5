using System.Collections.Generic;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class ProveedorCrudVM
    {
        public int ID_PROVEEDOR { get; set; }
        public string NOMBRE { get; set; }
        public string CONTACTO { get; set; }
        public string TELEFONO { get; set; }
        public string DATOS_FISCALES { get; set; }
        public int ID_ESTADO { get; set; }

        // Productos asociados (IDs separados por coma)
        public string PRODUCTOS_ASOCIADOS { get; set; }

        public List<ProveedorFilaVM> Proveedores { get; set; }
        public List<SelectListItem> Estados { get; set; }
        public List<SelectListItem> MaterialesDisponibles { get; set; }

        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalItems / PageSize);
    }

    public class ProveedorFilaVM
    {
        public int ID_PROVEEDOR { get; set; }
        public string NOMBRE { get; set; }
        public string CONTACTO { get; set; }
        public string TELEFONO { get; set; }
        public int ID_ESTADO { get; set; }
        public string ESTADO { get; set; }
        public int CANTIDAD_MATERIALES { get; set; }
        public string MATERIALES_NOMBRES { get; set; }
    }
}
