using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class MaterialesController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        private const decimal STOCK_MINIMO_DEFAULT = 10m;

        public ActionResult Index()
        {
            try
            {
                var activoId = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo")?.ID_ESTADO ?? 1;

                var vm = new MaterialCrudVM
                {
                    Materiales = db.MATERIALES
                        .OrderBy(m => m.NOMBRE)
                        .Select(m => new MaterialFilaVM
                        {
                            ID_MATERIAL = m.ID_MATERIAL,
                            NOMBRE = m.NOMBRE,
                            TIPO = m.TIPO,
                            STOCK = m.STOCK,
                            COSTO_UNITARIO = m.COSTO_UNITARIO,
                            ID_PROVEEDOR = m.ID_PROVEEDOR,
                            PROVEEDOR = m.PROVEEDORES.NOMBRE,
                            ID_ESTADO = m.ID_ESTADO,
                            ESTADO = m.ESTADO.NOMBRE
                        })
                        .ToList(),

                    Proveedores = db.PROVEEDORES
                        .Where(p => p.ID_ESTADO == activoId)
                        .OrderBy(p => p.NOMBRE)
                        .ToList()
                        .Select(p => new SelectListItem
                        {
                            Value = p.ID_PROVEEDOR.ToString(),
                            Text = p.NOMBRE
                        })
                        .ToList(),

                    Estados = db.ESTADO
                        .OrderBy(e => e.ID_ESTADO)
                        .ToList()
                        .Select(e => new SelectListItem
                        {
                            Value = e.ID_ESTADO.ToString(),
                            Text = e.NOMBRE
                        })
                        .ToList()
                };

                // Marcar stock bajo
                foreach (var mat in vm.Materiales)
                {
                    mat.STOCK_BAJO = mat.STOCK < STOCK_MINIMO_DEFAULT;
                    mat.STOCK_MINIMO = STOCK_MINIMO_DEFAULT;
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "No se pudo conectar a la base de datos. " + ex.Message;
                var vm = new MaterialCrudVM
                {
                    Materiales = new List<MaterialFilaVM>(),
                    Proveedores = new List<SelectListItem>(),
                    Estados = new List<SelectListItem>()
                };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MaterialCrudVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.NOMBRE) || string.IsNullOrWhiteSpace(vm.TIPO) || vm.ID_PROVEEDOR <= 0)
            {
                TempData["Mensaje"] = "Todos los campos son requeridos. Verifique nombre, tipo, proveedor y costo.";
                return RedirectToAction("Index");
            }

            // Validar código no repetido (nombre único como código)
            var existe = db.MATERIALES.Any(m => m.NOMBRE.Trim() == vm.NOMBRE.Trim());
            if (existe)
            {
                TempData["Mensaje"] = "El código ya está en uso. Ya existe un material con ese nombre.";
                return RedirectToAction("Index");
            }

            var estadoActivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo");

            var mat = new MATERIALES
            {
                NOMBRE = vm.NOMBRE.Trim(),
                TIPO = vm.TIPO.Trim(),
                STOCK = vm.STOCK,
                COSTO_UNITARIO = vm.COSTO_UNITARIO,
                ID_PROVEEDOR = vm.ID_PROVEEDOR,
                ID_ESTADO = estadoActivo != null ? estadoActivo.ID_ESTADO : 1
            };

            db.MATERIALES.Add(mat);
            db.SaveChanges();

            // Registrar en bitácora
            RegistrarBitacora("CREAR_MATERIAL", "Material registrado: " + mat.NOMBRE + " (ID: " + mat.ID_MATERIAL + ")");

            TempData["OK"] = "Material registrado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MaterialCrudVM vm)
        {
            var mat = db.MATERIALES.Find(vm.ID_MATERIAL);
            if (mat == null) return RedirectToAction("Index");

            if (string.IsNullOrWhiteSpace(vm.NOMBRE) || string.IsNullOrWhiteSpace(vm.TIPO) || vm.ID_PROVEEDOR <= 0)
            {
                TempData["Mensaje"] = "Todos los campos son requeridos.";
                return RedirectToAction("Index");
            }

            // Validar nombre único excluyendo el actual
            var existe = db.MATERIALES.Any(m => m.NOMBRE.Trim() == vm.NOMBRE.Trim() && m.ID_MATERIAL != vm.ID_MATERIAL);
            if (existe)
            {
                TempData["Mensaje"] = "El código ya está en uso. Ya existe otro material con ese nombre.";
                return RedirectToAction("Index");
            }

            mat.NOMBRE = vm.NOMBRE.Trim();
            mat.TIPO = vm.TIPO.Trim();
            mat.STOCK = vm.STOCK;
            mat.COSTO_UNITARIO = vm.COSTO_UNITARIO;
            mat.ID_PROVEEDOR = vm.ID_PROVEEDOR;
            mat.ID_ESTADO = vm.ID_ESTADO;

            db.SaveChanges();

            // Registrar fecha y usuario de modificación en bitácora
            var usuario = (Session["NombreUsuario"] ?? "Sistema").ToString();
            RegistrarBitacora("EDITAR_MATERIAL", "Material editado: " + mat.NOMBRE + " (ID: " + mat.ID_MATERIAL + ") por " + usuario + " el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            TempData["OK"] = "Material actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var mat = db.MATERIALES.Find(id);
            if (mat == null) return RedirectToAction("Index");

            // Verificar si tiene transacciones activas (movimientos, cortes, desperdicios, producto_material)
            bool tieneMovimientos = db.MOVIMIENTOS_INVENTARIO.Any(m => m.ID_MATERIAL == id);
            bool tieneCortes = db.REGISTRO_CORTES.Any(r => r.ID_MATERIAL == id);
            bool tieneDesperdicios = db.DESPERDICIOS_MATERIAL.Any(d => d.ID_MATERIAL == id);
            bool tieneProductos = db.PRODUCTO_MATERIAL.Any(pm => pm.ID_MATERIAL == id);

            if (tieneMovimientos || tieneCortes || tieneDesperdicios || tieneProductos)
            {
                TempData["Mensaje"] = "No se puede eliminar este material porque tiene registros de compra, uso o transacciones activas asociadas.";
                return RedirectToAction("Index");
            }

            db.MATERIALES.Remove(mat);
            db.SaveChanges();

            RegistrarBitacora("ELIMINAR_MATERIAL", "Material eliminado: " + mat.NOMBRE + " (ID: " + id + ")");

            TempData["OK"] = "Material eliminado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AjustarStock(int ID_MATERIAL, decimal AJUSTE_CANTIDAD, string AJUSTE_OBSERVACION)
        {
            var mat = db.MATERIALES.Find(ID_MATERIAL);
            if (mat == null)
            {
                TempData["Mensaje"] = "Material no encontrado.";
                return RedirectToAction("Index");
            }

            if (AJUSTE_CANTIDAD == 0)
            {
                TempData["Mensaje"] = "La cantidad de ajuste no puede ser cero.";
                return RedirectToAction("Index");
            }

            // Verificar que el stock resultante no sea negativo
            if (mat.STOCK + AJUSTE_CANTIDAD < 0)
            {
                TempData["Mensaje"] = "El ajuste resultaría en stock negativo. Stock actual: " + mat.STOCK;
                return RedirectToAction("Index");
            }

            // Registrar movimiento de ajuste
            var movimiento = new MOVIMIENTOS_INVENTARIO
            {
                ID_MATERIAL = ID_MATERIAL,
                TIPO_MOVIMIENTO = AJUSTE_CANTIDAD > 0 ? "ENTRADA_AJUSTE" : "SALIDA_AJUSTE",
                CANTIDAD = Math.Abs(AJUSTE_CANTIDAD),
                FECHA = DateTime.Now,
                OBSERVACION = string.IsNullOrWhiteSpace(AJUSTE_OBSERVACION)
                    ? "Ajuste manual de existencias"
                    : AJUSTE_OBSERVACION.Trim()
            };

            db.MOVIMIENTOS_INVENTARIO.Add(movimiento);

            mat.STOCK += AJUSTE_CANTIDAD;
            db.SaveChanges();

            var usuario = (Session["NombreUsuario"] ?? "Sistema").ToString();
            RegistrarBitacora("AJUSTE_STOCK", "Ajuste de stock en material " + mat.NOMBRE + " (ID: " + ID_MATERIAL + "): " + (AJUSTE_CANTIDAD > 0 ? "+" : "") + AJUSTE_CANTIDAD + " por " + usuario);

            TempData["OK"] = "Ajuste de existencias realizado correctamente. Nuevo stock: " + mat.STOCK;
            return RedirectToAction("Index");
        }

        private void RegistrarBitacora(string accion, string descripcion)
        {
            try
            {
                var idUsuario = Session["IdUsuario"] != null ? (int)Session["IdUsuario"] : 1;
                var bitacora = new BITACORA
                {
                    ID_USUARIO = idUsuario,
                    ACCION = accion,
                    DESCRIPCION = descripcion,
                    FECHA = DateTime.Now
                };
                db.BITACORA.Add(bitacora);
                db.SaveChanges();
            }
            catch
            {
                // No bloquear operación si falla la bitácora
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
