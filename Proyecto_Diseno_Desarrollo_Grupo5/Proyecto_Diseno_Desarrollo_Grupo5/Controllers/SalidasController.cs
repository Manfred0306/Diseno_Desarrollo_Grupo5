using System;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{ 
    public class SalidasController : Controller 
    { 
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        //Get: Salidas
        public ActionResult Index()
        {
            var salidas = db.MOVIMIENTOS_INVENTARIO
                .Where(m => m.TIPO_MOVIMIENTO == "SALIDA")
                .OrderByDescending(m => m.FECHA)
                .ToList();

            return View(salidas);
        }

        // GET: Salidas/Create
        public ActionResult Create()
        {
            ViewBag.ID_MATERIAL = new SelectList(db.MATERIALES, "ID_MATERIAL", "NOMBRE");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MOVIMIENTOS_INVENTARIO movimiento)
        {
            if (ModelState.IsValid)
            {
                // Buscar material
                var material = db.MATERIALES.Find(movimiento.ID_MATERIAL);

                if (material == null)
                {
                    ModelState.AddModelError("", "Material no encontrado.");
                }
                else if (movimiento.CANTIDAD > material.STOCK)
                {
                    ModelState.AddModelError("", "La cantidad supera el stock disponible.");
                }
                else
                {
                    // Registrar salida
                    movimiento.TIPO_MOVIMIENTO = "SALIDA";
                    movimiento.FECHA = DateTime.Now;

                    db.MOVIMIENTOS_INVENTARIO.Add(movimiento);

                    // Restar stock
                    material.STOCK -= movimiento.CANTIDAD;

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.ID_MATERIAL = new SelectList(db.MATERIALES, "ID_MATERIAL", "NOMBRE", movimiento.ID_MATERIAL);
            return View(movimiento);
        }
    }
}