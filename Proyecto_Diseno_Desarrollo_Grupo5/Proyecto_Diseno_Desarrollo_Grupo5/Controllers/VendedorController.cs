using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(2)]
    public class VendedorController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}