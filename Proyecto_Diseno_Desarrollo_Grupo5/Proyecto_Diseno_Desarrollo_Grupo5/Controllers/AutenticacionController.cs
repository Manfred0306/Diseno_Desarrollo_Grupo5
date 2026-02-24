using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class AutenticacionController : Controller
    {
        #region login

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Autenticacion");
        }

        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.OK = TempData["OK"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AutenticacionModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Revisá los campos. Hay información inválida.";
                return View(model);
            }

            using (var context = new DBGRUPO5Entities())
            {
                var res = context.USUARIO_LOGIN_SP(model.Correo, model.Contrasena).FirstOrDefault();

                if (res != null)
                {
                    Session["IdUsuario"] = res.ID_USUARIO;
                    Session["NombreUsuario"] = res.NOMBRE;
                    Session["IdRolFk"] = res.ID_ROL;
                    Session["Rol"] = res.ROL;

                    // ✅ Redirección según rol
                    if (res.ROL == "ADMINISTRADOR") return RedirectToAction("Index", "Home");
                    if (res.ROL == "VENDEDOR") return RedirectToAction("Index", "Vendedor");
                    return RedirectToAction("Index", "Home");
                }

                // ✅ Para que se muestre SweetAlert (error)
                ViewBag.Mensaje = "Correo o contraseña incorrectos, o tu usuario está inactivo.";
                return View(model);
            }
        }

        #endregion


        #region registro

        [HttpGet]
        public ActionResult Registro()
        {
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.OK = TempData["OK"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(AutenticacionModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Revisá los campos, hay información que no es valida.";
                return View(model);
            }

            var rol = model.IdRol ?? 3;
            var estado = model.IdEstado ?? 1;

            using (var context = new DBGRUPO5Entities())
            {
                var resultado = new System.Data.Entity.Core.Objects.ObjectParameter("Resultado", typeof(int));

                context.USUARIO_REGISTRAR_SP(
                    model.Nombre,
                    model.Correo,
                    model.Contrasena,
                    rol,
                    estado,
                    resultado
                );

                int res = (int)resultado.Value;

                if (res == 1)
                {
                    TempData["OK"] = "Cuenta creada correctamente, ahora podés ve a iniciar sesión.";
                    return RedirectToAction("Login", "Autenticacion");
                }

                if (res == -1)
                {
                    ViewBag.Mensaje = "Ese correo ya está registrado en el sistema, usá otro o iniciá sesión.";
                    return View(model);
                }

                ViewBag.Mensaje = "No se pudo registrar.";
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Perfil()
        {
            if (Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Autenticacion");

            int idUsuario = (int)Session["IdUsuario"];

            using (var context = new DBGRUPO5Entities())
            {
                var u = context.USUARIOS.FirstOrDefault(x => x.ID_USUARIO == idUsuario);
                if (u == null)
                    return RedirectToAction("Login", "Autenticacion");

                var vm = new PerfilVM
                {
                    IdUsuario = u.ID_USUARIO,
                    Nombre = u.NOMBRE,
                    Correo = u.CORREO,
                    Rol = (Session["Rol"] ?? "").ToString(),
                    Estado = (u.ID_ESTADO == 1) ? "ACTIVO" : "INACTIVO"
                };

                return View(vm);
            }
        }

            #endregion
        }
    }
