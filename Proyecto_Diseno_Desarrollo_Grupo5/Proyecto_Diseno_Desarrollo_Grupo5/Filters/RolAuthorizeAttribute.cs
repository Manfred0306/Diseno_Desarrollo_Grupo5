using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Filters
{
    public class RolAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _rolesPermitidos;

        public RolAuthorizeAttribute(params string[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos ?? Array.Empty<string>();
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var session = httpContext?.Session;
            if (session == null) return false;

            if (session["IdUsuario"] == null) return false;

            if (_rolesPermitidos.Length == 0) return true;

            var rolNombre = session["RolNombre"]?.ToString();
            if (string.IsNullOrWhiteSpace(rolNombre)) return false;

            return _rolesPermitidos.Contains(rolNombre);
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