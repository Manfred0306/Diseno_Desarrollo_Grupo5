using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Data;
using System.Data.SqlClient;
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
                    if (res.ROL == "ADMINISTRADOR") return RedirectToAction("Index", "Admin");
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

        #endregion

        #region perfil

        [HttpGet]
        public ActionResult Perfil()
        {
            if (Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Autenticacion");

            int idUsuario = (int)Session["IdUsuario"];

            using (var context = new DBGRUPO5Entities())
            {
                var model = context.Database.SqlQuery<UsuariosModel>(
                    "EXEC dbo.SP_USUARIO_PERFIL_OBTENER @ID_USUARIO",
                    new SqlParameter("@ID_USUARIO", idUsuario)
                ).FirstOrDefault();

                if (model == null)
                    return RedirectToAction("Login", "Autenticacion");

                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.OK = TempData["OK"];

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Perfil(UsuariosModel model)
        {
            if (Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Autenticacion");

            int idUsuario = (int)Session["IdUsuario"];

            if (string.IsNullOrWhiteSpace(model.Nombre) || model.Nombre.Trim().Length < 2)
            {
                TempData["Mensaje"] = "El nombre debe tener al menos 2 caracteres.";
                return RedirectToAction("Perfil");
            }

            if (string.IsNullOrWhiteSpace(model.Correo) || !model.Correo.Contains("@"))
            {
                TempData["Mensaje"] = "Ingresá un correo válido.";
                return RedirectToAction("Perfil");
            }

            bool quiereCambiarPass = !string.IsNullOrWhiteSpace(model.Contrasena);

            if (quiereCambiarPass)
            {
                if (model.Contrasena.Length < 4)
                {
                    TempData["Mensaje"] = "La nueva contraseña debe tener mínimo 4 caracteres.";
                    return RedirectToAction("Perfil");
                }

                if (model.Contrasena != model.ConfirmarContrasena)
                {
                    TempData["Mensaje"] = "La confirmación de contraseña no coincide.";
                    return RedirectToAction("Perfil");
                }
            }

            using (var context = new DBGRUPO5Entities())
            {
                var okParam = new SqlParameter("@OK", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };

                var msgParam = new SqlParameter("@MSG", SqlDbType.NVarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };

                object passActual = (object)model.ContrasenaActual ?? DBNull.Value;
                object passNueva = quiereCambiarPass ? (object)model.Contrasena : DBNull.Value;

                context.Database.ExecuteSqlCommand(
                    "EXEC dbo.SP_USUARIO_PERFIL_ACTUALIZAR " +
                    "@ID_USUARIO, @NOMBRE, @CORREO, @CONTRASENA_ACTUAL, @CONTRASENA_NUEVA, @OK OUTPUT, @MSG OUTPUT",
                    new SqlParameter("@ID_USUARIO", idUsuario),
                    new SqlParameter("@NOMBRE", model.Nombre.Trim()),
                    new SqlParameter("@CORREO", model.Correo.Trim()),
                    new SqlParameter("@CONTRASENA_ACTUAL", passActual),
                    new SqlParameter("@CONTRASENA_NUEVA", passNueva),
                    okParam,
                    msgParam
                );

                bool ok = (bool)okParam.Value;
                string msg = msgParam.Value.ToString();

                if (!ok)
                {
                    TempData["Mensaje"] = msg;
                    return RedirectToAction("Perfil");
                }

                Session["NombreUsuario"] = model.Nombre.Trim();

                TempData["OK"] = msg;
                return RedirectToAction("Perfil");
            }
        }

        #endregion
    }
}
