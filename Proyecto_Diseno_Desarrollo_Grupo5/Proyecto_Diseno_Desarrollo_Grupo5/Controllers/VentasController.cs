using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class VentasController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        // HISTORIAL
        public ActionResult Index(string q = null)
        {
            q = (q ?? "").Trim();

            var ventas = db.VENTAS.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                if (int.TryParse(q, out int n))
                    ventas = ventas.Where(v => v.ID_VENTA == n || v.ID_CLIENTE == n);
                else
                    ventas = ventas.Where(v => v.CLIENTES.NOMBRE.Contains(q));
            }

            var pagosPorVenta = db.PAGOS
                .GroupBy(p => p.ID_VENTA)
                .Select(g => new { IdVenta = g.Key, Pagado = g.Sum(x => x.MONTO) });

            var lista = (from v in ventas
                         join p in pagosPorVenta on v.ID_VENTA equals p.IdVenta into pj
                         from p in pj.DefaultIfEmpty()
                         orderby v.ID_VENTA descending
                         select new VentaFilaVM
                         {
                             IdVenta = v.ID_VENTA,
                             Cliente = v.CLIENTES.NOMBRE,
                             Fecha = v.FECHA,
                             Total = v.TOTAL,
                             Pagado = (p == null ? 0 : p.Pagado),
                             Saldo = v.TOTAL - (p == null ? 0 : p.Pagado),
                             IdEstado = v.ID_ESTADO,
                             Estado = v.ESTADO.NOMBRE
                         }).ToList();

            ViewBag.Q = q;
            return View(lista);
        }

        // FORM CREAR
        [HttpGet]
        public ActionResult Create()
        {
            var vm = new VentaCrearVM
            {
                Clientes = db.CLIENTES
                    .Where(c => c.ID_ESTADO == 1)
                    .OrderBy(c => c.NOMBRE)
                    .ToList()
                    .Select(c => new SelectListItem { Value = c.ID_CLIENTE.ToString(), Text = c.NOMBRE })
                    .ToList(),

                Productos = db.PRODUCTOS
                    .Where(p => p.ID_ESTADO == 1)
                    .OrderBy(p => p.NOMBRE)
                    .ToList()
                    .Select(p => new SelectListItem { Value = p.ID_PRODUCTO.ToString(), Text = p.NOMBRE })
                    .ToList()
            };

            return View(vm);
        }

        // GUARDAR VENTA + DESCONTAR INVENTARIO (MATERIALES)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int IdCliente, int[] IdProducto, decimal[] Cantidad, decimal[] PrecioUnitario)
        {
            if (IdCliente <= 0 || IdProducto == null || IdProducto.Length == 0)
            {
                TempData["ERR"] = "Debés seleccionar cliente y agregar al menos un producto.";
                return RedirectToAction("Create");
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var venta = new VENTAS
                    {
                        ID_CLIENTE = IdCliente,
                        FECHA = DateTime.Now,
                        TOTAL = 0,
                        ID_ESTADO = 1
                    };

                    db.VENTAS.Add(venta);
                    db.SaveChanges();

                    decimal total = 0;

                    for (int i = 0; i < IdProducto.Length; i++)
                    {
                        var idProd = IdProducto[i];
                        var cant = Cantidad[i];
                        var precio = PrecioUnitario[i];

                        if (cant <= 0) continue;
                        if (precio < 0) precio = 0;

                        var subtotal = cant * precio;
                        total += subtotal;

                        db.DETALLES_VENTAS.Add(new DETALLES_VENTAS
                        {
                            ID_VENTA = venta.ID_VENTA,
                            ID_PRODUCTO = idProd,
                            CANTIDAD = cant,
                            PRECIO_UNITARIO = precio,
                            SUBTOTAL = subtotal
                        });

                        // 🔥 Descontar inventario real (materiales) usando receta PRODUCTO_MATERIAL
                        var receta = db.PRODUCTO_MATERIAL
                            .Where(pm => pm.ID_PRODUCTO == idProd)
                            .ToList();

                        foreach (var r in receta)
                        {
                            var material = db.MATERIALES.Find(r.ID_MATERIAL);
                            if (material == null) continue;

                            // cantidad a descontar = cant vendida * cantidad usada por producto
                            var descuento = cant * r.CANTIDAD_USADA;

                            if (material.STOCK < descuento)
                                throw new Exception($"Stock insuficiente en material: {material.NOMBRE}. Disponible: {material.STOCK}, requerido: {descuento}");

                            material.STOCK -= descuento;
                        }
                    }

                    venta.TOTAL = total;
                    db.SaveChanges();

                    tx.Commit();
                    TempData["OK"] = $"Venta #{venta.ID_VENTA} registrada correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["ERR"] = ex.Message;
                    return RedirectToAction("Create");
                }
            }
        }

        // REGISTRAR PAGO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pagar(PagoCrearVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["ERR"] = "Revisá los datos del pago.";
                return RedirectToAction("Index");
            }

            var venta = db.VENTAS.Find(vm.IdVenta);
            if (venta == null)
            {
                TempData["ERR"] = "La venta no existe.";
                return RedirectToAction("Index");
            }

            if (venta.ID_ESTADO != 1)
            {
                TempData["ERR"] = "No se puede pagar una venta cancelada/inactiva.";
                return RedirectToAction("Index");
            }

            int? idUsuario = Session["IdUsuario"] as int?;

            db.PAGOS.Add(new PAGOS
            {
                ID_VENTA = vm.IdVenta,
                MONTO = vm.Monto,
                METODO = (vm.Metodo ?? "").Trim(),
                REFERENCIA = (vm.Referencia ?? "").Trim(),
                FECHA = DateTime.Now,
                ID_USUARIO = idUsuario
            });

            db.SaveChanges();
            TempData["OK"] = "Pago registrado.";
            return RedirectToAction("Index");
        }

        // CANCELAR VENTA + REVERTIR INVENTARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int id)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var venta = db.VENTAS
                        .Include(v => v.DETALLES_VENTAS)
                        .FirstOrDefault(v => v.ID_VENTA == id);

                    if (venta == null)
                    {
                        TempData["ERR"] = "Venta no encontrada.";
                        return RedirectToAction("Index");
                    }

                    if (venta.ID_ESTADO == 2)
                    {
                        TempData["ERR"] = "La venta ya está cancelada.";
                        return RedirectToAction("Index");
                    }

                    // 🔥 Revertir inventario
                    foreach (var det in venta.DETALLES_VENTAS.ToList())
                    {
                        var receta = db.PRODUCTO_MATERIAL
                            .Where(pm => pm.ID_PRODUCTO == det.ID_PRODUCTO)
                            .ToList();

                        foreach (var r in receta)
                        {
                            var material = db.MATERIALES.Find(r.ID_MATERIAL);
                            if (material == null) continue;

                            var devolucion = det.CANTIDAD * r.CANTIDAD_USADA;
                            material.STOCK += devolucion;
                        }
                    }

                    // Marcar venta como inactiva (cancelada)
                    venta.ID_ESTADO = 2;
                    db.SaveChanges();

                    tx.Commit();
                    TempData["OK"] = $"Venta #{id} cancelada (inventario revertido).";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["ERR"] = ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }
    }
}