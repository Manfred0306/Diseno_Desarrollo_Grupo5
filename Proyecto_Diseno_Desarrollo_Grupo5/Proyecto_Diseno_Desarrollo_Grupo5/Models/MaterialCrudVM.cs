using System.Collections.Generic;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class MaterialCrudVM
    {
        public int ID_MATERIAL { get; set; }
        public string NOMBRE { get; set; }
        public string TIPO { get; set; }
        public decimal STOCK { get; set; }
        public decimal COSTO_UNITARIO { get; set; }
        public int ID_PROVEEDOR { get; set; }
        public int ID_ESTADO { get; set; }
        public string UNIDAD_MEDIDA { get; set; }
        public decimal STOCK_MINIMO { get; set; }

        // Para ajuste manual de existencias
        public decimal AJUSTE_CANTIDAD { get; set; }
        public string AJUSTE_OBSERVACION { get; set; }

        public List<MaterialFilaVM> Materiales { get; set; }
        public List<SelectListItem> Proveedores { get; set; }
        public List<SelectListItem> Estados { get; set; }

        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalItems / PageSize);
    }

    public class MaterialFilaVM
    {
        public int ID_MATERIAL { get; set; }
        public string NOMBRE { get; set; }
        public string TIPO { get; set; }
        public decimal STOCK { get; set; }
        public decimal COSTO_UNITARIO { get; set; }
        public int ID_PROVEEDOR { get; set; }
        public string PROVEEDOR { get; set; }
        public int ID_ESTADO { get; set; }
        public string ESTADO { get; set; }
        public string UNIDAD_MEDIDA { get; set; }
        public decimal STOCK_MINIMO { get; set; }
        public bool STOCK_BAJO { get; set; }
        public string FECHA_MODIFICACION { get; set; }
        public string USUARIO_MODIFICACION { get; set; }
    }
}
