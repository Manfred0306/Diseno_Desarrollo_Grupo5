using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1)]
    public class UsuariosController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        // ============================================
        // LISTAR (SP_USUARIOS_LISTAR)
        // ============================================
        public ActionResult Index(string q = "", int rol = 0, int estado = 0)
        {
            var lista = db.Database.SqlQuery<UsuariosModel>(
                "EXEC dbo.SP_USUARIOS_LISTAR @Q, @ID_ROL, @ID_ESTADO",
                new SqlParameter("@Q", (object)q ?? DBNull.Value),
                new SqlParameter("@ID_ROL", rol),
                new SqlParameter("@ID_ESTADO", estado)
            ).ToList();

            ViewBag.Roles = db.ROLES.ToList();
            ViewBag.Q = q;
            ViewBag.Rol = rol;
            ViewBag.Estado = estado;

            return View(lista);
        }

        // ============================================
        // INSERTAR (SP_USUARIOS_INSERTAR)
        // ============================================
        [HttpPost]
        public ActionResult Create(UsuariosModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false, msg = "Datos inválidos." });

            var okParam = new SqlParameter("@OK", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            var msgParam = new SqlParameter("@MSG", SqlDbType.NVarChar, 200)
            {
                Direction = ParameterDirection.Output
            };

            db.Database.ExecuteSqlCommand(
                "EXEC dbo.SP_USUARIOS_INSERTAR @NOMBRE, @CORREO, @CONTRASENA, @ID_ROL, @OK OUTPUT, @MSG OUTPUT",
                new SqlParameter("@NOMBRE", model.Nombre),
                new SqlParameter("@CORREO", model.Correo),
                new SqlParameter("@CONTRASENA", model.Contrasena ?? ""),
                new SqlParameter("@ID_ROL", model.IdRol),
                okParam,
                msgParam
            );

            return Json(new
            {
                ok = (bool)okParam.Value,
                msg = msgParam.Value.ToString()
            });
        }

        // ============================================
        // ACTUALIZAR (SP_USUARIOS_ACTUALIZAR)
        // ============================================
        [HttpPost]
        public ActionResult Update(UsuariosModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false, msg = "Datos inválidos." });

            var okParam = new SqlParameter("@OK", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            var msgParam = new SqlParameter("@MSG", SqlDbType.NVarChar, 200)
            {
                Direction = ParameterDirection.Output
            };

            db.Database.ExecuteSqlCommand(
                "EXEC dbo.SP_USUARIOS_ACTUALIZAR @ID_USUARIO, @NOMBRE, @CORREO, @ID_ROL, @CONTRASENA, @OK OUTPUT, @MSG OUTPUT",
                new SqlParameter("@ID_USUARIO", model.IdUsuario),
                new SqlParameter("@NOMBRE", model.Nombre),
                new SqlParameter("@CORREO", model.Correo),
                new SqlParameter("@ID_ROL", model.IdRol),
                new SqlParameter("@CONTRASENA", (object)model.Contrasena ?? DBNull.Value),
                okParam,
                msgParam
            );

            return Json(new
            {
                ok = (bool)okParam.Value,
                msg = msgParam.Value.ToString()
            });
        }

        // ============================================
        // TOGGLE ESTADO (SP_USUARIOS_TOGGLE_ESTADO)
        // ============================================
        [HttpPost]
        public ActionResult ToggleEstado(int id)
        {
            var okParam = new SqlParameter("@OK", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            var msgParam = new SqlParameter("@MSG", SqlDbType.NVarChar, 200)
            {
                Direction = ParameterDirection.Output
            };

            db.Database.ExecuteSqlCommand(
                "EXEC dbo.SP_USUARIOS_TOGGLE_ESTADO @ID_USUARIO, @OK OUTPUT, @MSG OUTPUT",
                new SqlParameter("@ID_USUARIO", id),
                okParam,
                msgParam
            );

            return Json(new
            {
                ok = (bool)okParam.Value,
                msg = msgParam.Value.ToString()
            });
        }
    }
}