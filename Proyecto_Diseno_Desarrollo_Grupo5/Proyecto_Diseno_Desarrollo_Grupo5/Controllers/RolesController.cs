using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1)]
    public class RolesController : Controller
    {
        #region Index (Listar)
        [HttpGet]
        public ActionResult Index(string q = "", int estado = 0)
        {
            // Mensajes para SweetAlert
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.OK = TempData["OK"];

            q = (q ?? "").Trim();

            using (var context = new DBGRUPO5Entities())
            {
                var roles = context.ROLES.AsQueryable();

                // Buscar por nombre o descripción (descripción puede venir null)
                if (!string.IsNullOrEmpty(q))
                {
                    roles = roles.Where(r =>
                        r.NOMBRE.Contains(q) ||
                        (r.DESCRIPCION != null && r.DESCRIPCION.Contains(q))
                    );
                }

                // Estado: 0 = todos, 1 = activo, 2 = inactivo
                if (estado == 1 || estado == 2)
                {
                    roles = roles.Where(r => r.ID_ESTADO == estado);
                }

                var lista = roles
                    .OrderBy(r => r.ID_ROL)
                    .ToList();

                // Mantener filtros en la vista
                ViewBag.Q = q;
                ViewBag.Estado = estado;

                return View(lista);
            }
        }
        #endregion


        #region Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RolesModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Revisá los campos. Hay información inválida.";
                return RedirectToAction("Index");
            }

            var nombre = (model.Nombre ?? "").Trim().ToUpper();
            var descripcion = (model.Descripcion ?? "").Trim();
            var idEstado = (model.IdEstado == 2 ? 2 : 1);

            if (nombre.Length < 2)
            {
                TempData["Mensaje"] = "El nombre del rol debe tener al menos 2 caracteres.";
                return RedirectToAction("Index");
            }

            using (var context = new DBGRUPO5Entities())
            {
                // Evitar duplicados por nombre (ignorando mayúsculas/minúsculas)
                var existe = context.ROLES.Any(r => r.NOMBRE.ToUpper() == nombre);
                if (existe)
                {
                    TempData["Mensaje"] = "Ya existe un rol con ese nombre.";
                    return RedirectToAction("Index");
                }

                var rol = new ROLES
                {
                    NOMBRE = nombre,
                    DESCRIPCION = descripcion,
                    ID_ESTADO = idEstado
                };

                context.ROLES.Add(rol);
                context.SaveChanges();

                TempData["OK"] = "Rol creado correctamente.";
                return RedirectToAction("Index");
            }
        }
        #endregion


        #region Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(RolesModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Revisá los campos. Hay información inválida.";
                return RedirectToAction("Index");
            }

            if (model.IdRol == null || model.IdRol <= 0)
            {
                TempData["Mensaje"] = "ID de rol inválido.";
                return RedirectToAction("Index");
            }

            var nombre = (model.Nombre ?? "").Trim().ToUpper();
            var descripcion = (model.Descripcion ?? "").Trim();
            var idEstado = (model.IdEstado == 2 ? 2 : 1);

            if (nombre.Length < 2)
            {
                TempData["Mensaje"] = "El nombre del rol debe tener al menos 2 caracteres.";
                return RedirectToAction("Index");
            }

            using (var context = new DBGRUPO5Entities())
            {
                var rol = context.ROLES.FirstOrDefault(r => r.ID_ROL == model.IdRol);
                if (rol == null)
                {
                    TempData["Mensaje"] = "No se encontró el rol seleccionado.";
                    return RedirectToAction("Index");
                }

                // Evitar duplicado de nombre en otro rol
                var existe = context.ROLES.Any(r =>
                    r.ID_ROL != model.IdRol &&
                    r.NOMBRE.ToUpper() == nombre
                );

                if (existe)
                {
                    TempData["Mensaje"] = "Ya existe otro rol con ese nombre.";
                    return RedirectToAction("Index");
                }

                rol.NOMBRE = nombre;
                rol.DESCRIPCION = descripcion;
                rol.ID_ESTADO = idEstado;

                context.SaveChanges();

                TempData["OK"] = "Rol actualizado correctamente.";
                return RedirectToAction("Index");
            }
        }
        #endregion


        #region Deactivate (Borrado lógico -> INACTIVO)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            using (var context = new DBGRUPO5Entities())
            {
                var rol = context.ROLES.FirstOrDefault(r => r.ID_ROL == id);
                if (rol == null)
                {
                    TempData["Mensaje"] = "No se encontró el rol para inactivar.";
                    return RedirectToAction("Index");
                }

                rol.ID_ESTADO = 2; // INACTIVO
                context.SaveChanges();

                TempData["OK"] = "Rol inactivado correctamente.";
                return RedirectToAction("Index");
            }
        }
        #endregion


        #region Activate (Volver a ACTIVO)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(int id)
        {
            using (var context = new DBGRUPO5Entities())
            {
                var rol = context.ROLES.FirstOrDefault(r => r.ID_ROL == id);
                if (rol == null)
                {
                    TempData["Mensaje"] = "No se encontró el rol para activar.";
                    return RedirectToAction("Index");
                }

                rol.ID_ESTADO = 1; // ACTIVO
                context.SaveChanges();

                TempData["OK"] = "Rol activado correctamente.";
                return RedirectToAction("Index");
            }
        }
        #endregion
    }
}