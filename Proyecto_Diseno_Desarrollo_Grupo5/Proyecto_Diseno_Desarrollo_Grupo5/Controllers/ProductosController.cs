using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class ProductosController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        [RolAuthorize(1,2)]
        public ActionResult Index()
        {
            var vm = new ProductoCrudVM
            {
                Productos = db.PRODUCTOS
                    .OrderBy(p => p.NOMBRE)
                    .Select(p => new ProductoFilaVM
                    {
                        ID_PRODUCTO = p.ID_PRODUCTO,
                        NOMBRE = p.NOMBRE,
                        PRECIO_VENTA = p.PRECIO_VENTA,
                        ID_CATEGORIA = p.ID_CATEGORIA,
                        CATEGORIA = p.CATEGORIAS.NOMBRE,
                        ID_ESTADO = p.ID_ESTADO,
                        ESTADO = p.ESTADO.NOMBRE
                    })
                    .ToList(),

                Categorias = db.CATEGORIAS
                    .OrderBy(c => c.NOMBRE)
                    .ToList()
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_CATEGORIA.ToString(),
                        Text = c.NOMBRE
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

        [RolAuthorize(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductoCrudVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.NOMBRE))
                return RedirectToAction("Index");

            var p = new PRODUCTOS
            {
                NOMBRE = vm.NOMBRE.Trim(),
                PRECIO_VENTA = vm.PRECIO_VENTA,
                ID_CATEGORIA = vm.ID_CATEGORIA,
                ID_ESTADO = vm.ID_ESTADO
            };

            db.PRODUCTOS.Add(p);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [RolAuthorize(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductoCrudVM vm)
        {
            var p = db.PRODUCTOS.Find(vm.ID_PRODUCTO);
            if (p == null) return RedirectToAction("Index");

            p.NOMBRE = (vm.NOMBRE ?? "").Trim();
            p.PRECIO_VENTA = vm.PRECIO_VENTA;
            p.ID_CATEGORIA = vm.ID_CATEGORIA;
            p.ID_ESTADO = vm.ID_ESTADO;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [RolAuthorize(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var p = db.PRODUCTOS.Find(id);
            if (p == null) return RedirectToAction("Index");

            db.PRODUCTOS.Remove(p);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
