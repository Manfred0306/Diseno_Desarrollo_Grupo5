using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class VentaCrearItemVM
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class VentaCrearVM
    {
        [Required]
        public int IdCliente { get; set; }

        public List<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Productos { get; set; } = new List<SelectListItem>();

        public List<VentaCrearItemVM> Items { get; set; } = new List<VentaCrearItemVM>();
    }

    public class VentaFilaVM
    {
        public int IdVenta { get; set; }
        public string Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public decimal Pagado { get; set; }
        public decimal Saldo { get; set; }
        public int IdEstado { get; set; }
        public string Estado { get; set; }
    }

    public class PagoCrearVM
    {
        public int IdVenta { get; set; }

        [Range(0.01, 999999999)]
        public decimal Monto { get; set; }

        [Required]
        public string Metodo { get; set; }

        public string Referencia { get; set; }
    }
}