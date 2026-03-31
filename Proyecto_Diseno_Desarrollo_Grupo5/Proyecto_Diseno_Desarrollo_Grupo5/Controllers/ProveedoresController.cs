using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1)]
    public class ProveedoresController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index(string q = null, int page = 1, int pageSize = 10)
        {
            try
            {
                q = (q ?? "").Trim();
                var query = db.PROVEEDORES.AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(p => p.NOMBRE.Contains(q) || p.CONTACTO.Contains(q) || p.TELEFONO.Contains(q));
                }

                query = query.OrderBy(p => p.NOMBRE);

                var total = query.Count();
                var skip = (Math.Max(page, 1) - 1) * pageSize;
                var items = query.Skip(skip).Take(pageSize).ToList();

                var vm = new ProveedorCrudVM
                {
                    Q = q,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    Proveedores = items
                        .Select(p => new ProveedorFilaVM
                        {
                            ID_PROVEEDOR = p.ID_PROVEEDOR,
                            NOMBRE = p.NOMBRE,
                            CONTACTO = p.CONTACTO,
                            TELEFONO = p.TELEFONO,
                            ID_ESTADO = p.ID_ESTADO,
                            ESTADO = p.ESTADO.NOMBRE,
                            CANTIDAD_MATERIALES = p.MATERIALES.Count
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
                        .ToList(),

                    MaterialesDisponibles = db.MATERIALES
                        .OrderBy(m => m.NOMBRE)
                        .ToList()
                        .Select(m => new SelectListItem
                        {
                            Value = m.ID_MATERIAL.ToString(),
                            Text = m.NOMBRE
                        })
                        .ToList()
                };

                foreach (var prov in vm.Proveedores)
                {
                    var materiales = db.MATERIALES
                        .Where(m => m.ID_PROVEEDOR == prov.ID_PROVEEDOR)
                        .Select(m => m.NOMBRE)
                        .ToList();
                    prov.MATERIALES_NOMBRES = materiales.Any() ? string.Join(", ", materiales) : "Sin productos";
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "No se pudo conectar a la base de datos. " + ex.Message;
                var vm = new ProveedorCrudVM
                {
                    Proveedores = new List<ProveedorFilaVM>(),
                    Estados = new List<SelectListItem>(),
                    MaterialesDisponibles = new List<SelectListItem>()
                };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProveedorCrudVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.NOMBRE) || string.IsNullOrWhiteSpace(vm.CONTACTO) || string.IsNullOrWhiteSpace(vm.TELEFONO))
            {
                TempData["Mensaje"] = "Todos los campos son requeridos: nombre, contacto (datos fiscales) y tel?fono.";
                return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
            }

            var estadoActivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo");

            var prov = new PROVEEDORES
            {
                NOMBRE = vm.NOMBRE.Trim(),
                CONTACTO = vm.CONTACTO.Trim(),
                TELEFONO = vm.TELEFONO.Trim(),
                ID_ESTADO = estadoActivo != null ? estadoActivo.ID_ESTADO : 1
            };

            db.PROVEEDORES.Add(prov);
            db.SaveChanges();

            // Asociar materiales al proveedor solo si se seleccionaron
            if (!string.IsNullOrWhiteSpace(vm.PRODUCTOS_ASOCIADOS))
            {
                var idsStr = vm.PRODUCTOS_ASOCIADOS.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var idStr in idsStr)
                {
                    int matId;
                    if (int.TryParse(idStr.Trim(), out matId))
                    {
                        var material = db.MATERIALES.Find(matId);
                        if (material != null)
                        {
                            material.ID_PROVEEDOR = prov.ID_PROVEEDOR;
                        }
                    }
                }
                db.SaveChanges();
            }

            RegistrarBitacora("CREAR_PROVEEDOR", "Proveedor registrado: " + prov.NOMBRE + " (ID: " + prov.ID_PROVEEDOR + ")");

            TempData["OK"] = "Proveedor registrado correctamente.";
            return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProveedorCrudVM vm)
        {
            var prov = db.PROVEEDORES.Find(vm.ID_PROVEEDOR);
            if (prov == null) return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });

            if (string.IsNullOrWhiteSpace(vm.NOMBRE) || string.IsNullOrWhiteSpace(vm.CONTACTO) || string.IsNullOrWhiteSpace(vm.TELEFONO))
            {
                TempData["Mensaje"] = "Todos los campos son requeridos.";
                return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
            }

            prov.NOMBRE = vm.NOMBRE.Trim();
            prov.CONTACTO = vm.CONTACTO.Trim();
            prov.TELEFONO = vm.TELEFONO.Trim();
            prov.ID_ESTADO = vm.ID_ESTADO;

            // Actualizar materiales asociados
            if (!string.IsNullOrWhiteSpace(vm.PRODUCTOS_ASOCIADOS))
            {
                // Desasociar materiales actuales de este proveedor (ponerlos sin proveedor no es posible por FK, se dejan)
                var idsStr = vm.PRODUCTOS_ASOCIADOS.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var idsNuevos = new List<int>();
                foreach (var idStr in idsStr)
                {
                    int matId;
                    if (int.TryParse(idStr.Trim(), out matId))
                    {
                        idsNuevos.Add(matId);
                        var material = db.MATERIALES.Find(matId);
                        if (material != null)
                        {
                            material.ID_PROVEEDOR = prov.ID_PROVEEDOR;
                        }
                    }
                }
            }

            db.SaveChanges();

            RegistrarBitacora("EDITAR_PROVEEDOR", "Proveedor editado: " + prov.NOMBRE + " (ID: " + prov.ID_PROVEEDOR + ")");

            TempData["OK"] = "Proveedor actualizado correctamente.";
            return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id, int page = 1, string q = null)
        {
            var prov = db.PROVEEDORES.Find(id);
            if (prov == null) return RedirectToAction("Index", new { page, q });

            var estadoInactivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Inactivo");
            var inactivoId = estadoInactivo != null ? estadoInactivo.ID_ESTADO : 2;

            prov.ID_ESTADO = inactivoId;

            // Desactivar materiales asociados exclusivamente a este proveedor
            var materialesExclusivos = db.MATERIALES
                .Where(m => m.ID_PROVEEDOR == id)
                .ToList();

            foreach (var mat in materialesExclusivos)
            {
                // Verificar si este material solo tiene este proveedor (es exclusivo)
                // Como la relaci?n es 1-a-muchos (un material tiene un proveedor), si ID_PROVEEDOR == id, es exclusivo
                mat.ID_ESTADO = inactivoId;
            }

            db.SaveChanges();

            RegistrarBitacora("DESACTIVAR_PROVEEDOR", "Proveedor desactivado: " + prov.NOMBRE + " (ID: " + id + "). Materiales exclusivos tambi?n desactivados: " + materialesExclusivos.Count);

            TempData["OK"] = "Proveedor desactivado correctamente. " + materialesExclusivos.Count + " material(es) exclusivo(s) tambi?n desactivado(s).";
            return RedirectToAction("Index", new { page, q });
        }

        public ActionResult GetMaterialesByProveedor(int id)
        {
            var materiales = db.MATERIALES
                .Where(m => m.ID_PROVEEDOR == id)
                .Select(m => new { m.ID_MATERIAL, m.NOMBRE })
                .ToList();

            return Json(materiales, JsonRequestBehavior.AllowGet);
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
                // No bloquear operaci?n si falla la bit?cora
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
