using System.Collections.Generic;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class ProductoCrudVM
    {
        public int ID_PRODUCTO { get; set; }
        public string NOMBRE { get; set; }
        public decimal PRECIO_VENTA { get; set; }
        public int ID_CATEGORIA { get; set; }
        public int ID_ESTADO { get; set; }

        public List<ProductoFilaVM> Productos { get; set; }

        public List<SelectListItem> Categorias { get; set; }
        public List<SelectListItem> Estados { get; set; }
    }

    public class ProductoFilaVM
    {
        public int ID_PRODUCTO { get; set; }
        public string NOMBRE { get; set; }
        public decimal PRECIO_VENTA { get; set; }

        public int ID_CATEGORIA { get; set; }
        public string CATEGORIA { get; set; }

        public int ID_ESTADO { get; set; }
        public string ESTADO { get; set; }
    }

    public class ProductoVentaVM
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioVenta { get; set; }
    }
}
