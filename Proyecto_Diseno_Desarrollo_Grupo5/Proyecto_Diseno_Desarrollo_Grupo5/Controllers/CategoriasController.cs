using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1)]
    public class CategoriasController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var query = db.CATEGORIAS.OrderBy(c => c.NOMBRE).AsQueryable();

                var total = query.Count();
                var skip = (Math.Max(page, 1) - 1) * pageSize;
                var items = query.Skip(skip).Take(pageSize).ToList();

                var vm = new CategoriaCrudVM
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    Categorias = items
                        .Select(c => new CategoriaFilaVM
                        {
                            ID_CATEGORIA = c.ID_CATEGORIA,
                            NOMBRE = c.NOMBRE,
                            DESCRIPCION = c.DESCRIPCION,
                            ID_ESTADO = c.ID_ESTADO,
                            ESTADO = c.ESTADO.NOMBRE
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

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "No se pudo conectar a la base de datos. " + ex.Message;
                var vm = new CategoriaCrudVM
                {
                    Categorias = new System.Collections.Generic.List<CategoriaFilaVM>(),
                    Estados = new System.Collections.Generic.List<SelectListItem>()
                };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoriaCrudVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.NOMBRE))
            {
                TempData["Mensaje"] = "El nombre es requerido.";
                return RedirectToAction("Index", new { page = vm.Page });
            }

            var existe = db.CATEGORIAS.Any(c => c.NOMBRE == vm.NOMBRE.Trim());
            if (existe)
            {
                TempData["Mensaje"] = "Ya existe una categoría con ese nombre.";
                return RedirectToAction("Index", new { page = vm.Page });
            }

            var estadoActivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo");

            var cat = new CATEGORIAS
            {
                NOMBRE = vm.NOMBRE.Trim(),
                DESCRIPCION = (vm.DESCRIPCION ?? "").Trim(),
                ID_ESTADO = estadoActivo != null ? estadoActivo.ID_ESTADO : 1
            };

            db.CATEGORIAS.Add(cat);
            db.SaveChanges();

            TempData["OK"] = "Categoría registrada correctamente.";
            return RedirectToAction("Index", new { page = vm.Page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CategoriaCrudVM vm)
        {
            var cat = db.CATEGORIAS.Find(vm.ID_CATEGORIA);
            if (cat == null) return RedirectToAction("Index");

            if (string.IsNullOrWhiteSpace(vm.NOMBRE))
            {
                TempData["Mensaje"] = "El nombre es requerido.";
                return RedirectToAction("Index", new { page = vm.Page });
            }

            var existe = db.CATEGORIAS.Any(c => c.NOMBRE == vm.NOMBRE.Trim() && c.ID_CATEGORIA != vm.ID_CATEGORIA);
            if (existe)
            {
                TempData["Mensaje"] = "Ya existe otra categoría con ese nombre.";
                return RedirectToAction("Index", new { page = vm.Page });
            }

            cat.NOMBRE = vm.NOMBRE.Trim();
            cat.DESCRIPCION = (vm.DESCRIPCION ?? "").Trim();
            cat.ID_ESTADO = vm.ID_ESTADO;

            db.SaveChanges();

            TempData["OK"] = "Categoría actualizada correctamente.";
            return RedirectToAction("Index", new { page = vm.Page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            var cat = db.CATEGORIAS.Find(id);
            if (cat == null) return RedirectToAction("Index");

            var estadoInactivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Inactivo");
            cat.ID_ESTADO = estadoInactivo != null ? estadoInactivo.ID_ESTADO : 2;

            db.SaveChanges();

            TempData["OK"] = "Categoría puesta como Inactiva.";
            return RedirectToAction("Index", new { page = 1 });
        }

        public ActionResult GetActiveCategories()
        {
            var activoId = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo")?.ID_ESTADO ?? 1;
            var list = db.CATEGORIAS
                .Where(c => c.ID_ESTADO == activoId)
                .Select(c => new { c.ID_CATEGORIA, c.NOMBRE })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
