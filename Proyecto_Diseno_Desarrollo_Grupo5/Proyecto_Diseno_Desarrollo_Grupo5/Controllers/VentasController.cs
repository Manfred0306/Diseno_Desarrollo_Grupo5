using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.IO;
using System.Web;
using System.Globalization;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class VentasController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index(string q = null, int page = 1, int pageSize = 10)
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
                .Select(g => new
                {
                    IdVenta = g.Key,
                    Pagado = g.Sum(x => x.MONTO)
                });

            var lista = (from v in ventas
                         join p in pagosPorVenta
                            on v.ID_VENTA equals p.IdVenta into pagosJoin
                         from p in pagosJoin.DefaultIfEmpty()
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

            // Paginación server-side
            var total = lista.Count;
            var skip = (Math.Max(page, 1) - 1) * pageSize;
            var paged = lista.Skip(skip).Take(pageSize).ToList();

            ViewBag.Q = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

            return View(paged);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var productosActivos = db.PRODUCTOS
                .Where(p => p.ID_ESTADO == 1)
                .OrderBy(p => p.NOMBRE)
                .ToList();

            var vm = new VentaCrearVM
            {
                Productos = productosActivos
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_PRODUCTO.ToString(),
                        Text = p.NOMBRE
                    })
                    .ToList(),

                ProductosConPrecio = productosActivos
                    .Select(p => new ProductoVentaVM
                    {
                        IdProducto = p.ID_PRODUCTO,
                        Nombre = p.NOMBRE,
                        PrecioVenta = p.PRECIO_VENTA
                    })
                    .ToList()
            };

            return View(vm);
        }

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

                        var receta = db.PRODUCTO_MATERIAL
                            .Where(pm => pm.ID_PRODUCTO == idProd)
                            .ToList();

                        foreach (var r in receta)
                        {
                            var material = db.MATERIALES.Find(r.ID_MATERIAL);
                            if (material == null) continue;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pagar(PagoCrearVM vm, HttpPostedFileBase comprobanteSinpe)
        {
            // Leer el monto crudo del form para evitar problemas de cultura
            var montoRaw = (Request["Monto"] ?? "").Trim();

            decimal montoParseado;

            // Intentar con punto decimal
            bool okMonto =
                decimal.TryParse(montoRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out montoParseado)
                || decimal.TryParse(montoRaw, NumberStyles.Any, CultureInfo.CurrentCulture, out montoParseado);

            if (vm == null)
            {
                TempData["ERR"] = "No se recibieron datos del pago.";
                return RedirectToAction("Index");
            }

            if (vm.IdVenta <= 0)
            {
                TempData["ERR"] = "La venta no es válida.";
                return RedirectToAction("Index");
            }

            if (!okMonto || montoParseado <= 0)
            {
                TempData["ERR"] = "El monto ingresado no es válido.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(vm.Metodo))
            {
                TempData["ERR"] = "Debe seleccionar un método de pago.";
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
                TempData["ERR"] = "No se puede registrar pago a una venta cancelada.";
                return RedirectToAction("Index");
            }

            decimal totalPagado = db.PAGOS
                .Where(p => p.ID_VENTA == vm.IdVenta)
                .Select(p => (decimal?)p.MONTO)
                .Sum() ?? 0;

            decimal saldoActual = venta.TOTAL - totalPagado;

            if (saldoActual <= 0)
            {
                TempData["ERR"] = "Esta venta ya está completamente pagada.";
                return RedirectToAction("Index");
            }

            if (montoParseado > saldoActual)
            {
                TempData["ERR"] = $"El monto ingresado supera el saldo pendiente. Saldo actual: {saldoActual}";
                return RedirectToAction("Index");
            }

            string rutaImagen = null;
            var metodo = (vm.Metodo ?? "").Trim();

            if (metodo.Equals("Sinpe", StringComparison.OrdinalIgnoreCase))
            {
                if (comprobanteSinpe == null || comprobanteSinpe.ContentLength <= 0)
                {
                    TempData["ERR"] = "Debe adjuntar el comprobante de SINPE.";
                    return RedirectToAction("Index");
                }

                var extension = Path.GetExtension(comprobanteSinpe.FileName)?.ToLower();
                var extensionesValidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!extensionesValidas.Contains(extension))
                {
                    TempData["ERR"] = "El comprobante debe ser una imagen válida (.jpg, .jpeg, .png, .webp).";
                    return RedirectToAction("Index");
                }

                var nombreArchivo = $"sinpe_{vm.IdVenta}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var carpetaFisica = Server.MapPath("~/Uploads/Sinpe");

                if (!Directory.Exists(carpetaFisica))
                    Directory.CreateDirectory(carpetaFisica);

                var rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);
                comprobanteSinpe.SaveAs(rutaFisica);

                rutaImagen = "/Uploads/Sinpe/" + nombreArchivo;
            }

            int? idUsuario = null;
            if (Session["IdUsuario"] != null)
                idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            db.PAGOS.Add(new PAGOS
            {
                ID_VENTA = vm.IdVenta,
                MONTO = montoParseado,
                METODO = metodo,
                REFERENCIA = (vm.Referencia ?? "").Trim(),
                FECHA = DateTime.Now,
                ID_USUARIO = idUsuario,
                COMPROBANTE_IMG = rutaImagen
            });

            db.SaveChanges();

            TempData["OK"] = "Pago registrado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult BuscarClientes(string term)
        {
            term = (term ?? "").Trim().ToLower();

            var clientes = db.CLIENTES
                .Where(c =>
                    c.NOMBRE.ToLower().Contains(term) ||
                    (c.TELEFONO ?? "").ToLower().Contains(term) ||
                    (c.CORREO ?? "").ToLower().Contains(term)
                )
                .OrderBy(c => c.NOMBRE)
                .Take(10)
                .Select(c => new
                {
                    id = c.ID_CLIENTE,
                    nombre = c.NOMBRE,
                    telefono = c.TELEFONO,
                    correo = c.CORREO
                })
                .ToList();

            return Json(clientes, JsonRequestBehavior.AllowGet);
        }

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