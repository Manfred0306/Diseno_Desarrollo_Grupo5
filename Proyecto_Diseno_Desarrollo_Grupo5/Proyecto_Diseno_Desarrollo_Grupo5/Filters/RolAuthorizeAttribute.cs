using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Filters
{
    public class RolAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly int[] _rolesPermitidos;

        public RolAuthorizeAttribute(params int[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos ?? Array.Empty<int>();
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var session = httpContext?.Session;
            if (session == null) return false;

            if (session["IdUsuario"] == null) return false;

            if (_rolesPermitidos.Length == 0) return true;

            // Leemos el IdRol de la sesión
            if (session["IdRol"] == null) return false;

            int idRol;
            if (!int.TryParse(session["IdRol"].ToString(), out idRol)) return false;

            return _rolesPermitidos.Contains(idRol);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["IdUsuario"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "Autenticacion", action = "Login" })
                );
                return;
            }

            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(new { controller = "Autenticacion", action = "AccesoDenegado" })
            );
        }
    }
}