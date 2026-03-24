using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class ClientesController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index(string q = null, string sortOrder = null, int page = 1, int pageSize = 10)
        {
            ViewBag.NameSortParm = (sortOrder == "name_asc") ? "name_desc" : "name_asc";

            var clientesQuery = db.CLIENTES.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                if (int.TryParse(q, out int idBuscado))
                    clientesQuery = clientesQuery.Where(c => c.ID_CLIENTE == idBuscado);
                else
                    clientesQuery = clientesQuery.Where(c => c.NOMBRE.Contains(q)
                                                          || c.CORREO.Contains(q)
                                                          || c.TELEFONO.Contains(q));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    clientesQuery = clientesQuery.OrderByDescending(c => c.NOMBRE);
                    break;
                case "name_asc":
                default:
                    clientesQuery = clientesQuery.OrderBy(c => c.NOMBRE);
                    break;
            }

            // Count total items before paging
            var totalItems = clientesQuery.Count();

            // Apply paging
            var skip = (Math.Max(page, 1) - 1) * pageSize;
            var clientesPaged = clientesQuery.Skip(skip).Take(pageSize).ToList();

            var vm = new ClienteCrudVM
            {
                Q = q,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Clientes = clientesPaged.Select(c => new ClienteFilaVM
                {
                    ID_CLIENTE = c.ID_CLIENTE,
                    NOMBRE = c.NOMBRE,
                    TELEFONO = c.TELEFONO,
                    CORREO = c.CORREO,
                    DIRECCION = c.DIRECCION,
                    ID_ESTADO = c.ID_ESTADO,
                    ESTADO = c.ESTADO.NOMBRE
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClienteCrudVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.NOMBRE))
            {
                TempData["ERR"] = "El nombre del cliente es obligatorio.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(vm.CORREO) && !ModelState.IsValid)
            {
                TempData["ERR"] = "Correo inválido.";
                return RedirectToAction("Index");
            }

            var c = new CLIENTES
            {
                NOMBRE = vm.NOMBRE.Trim(),
                TELEFONO = (vm.TELEFONO ?? "").Trim(),
                CORREO = (vm.CORREO ?? "").Trim(),
                DIRECCION = (vm.DIRECCION ?? "").Trim(),
                ID_ESTADO = 1
            };

            db.CLIENTES.Add(c);
            db.SaveChanges();

            TempData["OK"] = "Cliente agregado de forma exitosa.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ClienteCrudVM vm)
        {
            var c = db.CLIENTES.Find(vm.ID_CLIENTE);
            if (c == null)
            {
                TempData["ERR"] = "Cliente no encontrado.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(vm.NOMBRE))
            {
                TempData["ERR"] = "El nombre del cliente es obligatorio.";
                return RedirectToAction("Index");
            }

            c.NOMBRE = vm.NOMBRE.Trim();
            c.TELEFONO = (vm.TELEFONO ?? "").Trim();
            c.CORREO = (vm.CORREO ?? "").Trim();
            c.DIRECCION = (vm.DIRECCION ?? "").Trim();

            if (vm.ID_ESTADO != 0)
                c.ID_ESTADO = vm.ID_ESTADO;

            db.SaveChanges();

            TempData["OK"] = "Cliente actualizado. Los cambios se muestran en la lista.";
            return RedirectToAction("Index", new { q = vm.Q, page = vm.Page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleEstado(int id, string q = null, string sortOrder = null, int page = 1)
        {
            var c = db.CLIENTES.Find(id);
            if (c == null)
            {
                TempData["ERR"] = "Cliente no encontrado.";
                return RedirectToAction("Index", new { q, sortOrder, page });
            }

            c.ID_ESTADO = (c.ID_ESTADO == 1) ? 2 : 1;
            db.SaveChanges();

            TempData["OK"] = "Estado del cliente actualizado.";
            return RedirectToAction("Index", new { q, sortOrder, page });
        }
    }
}