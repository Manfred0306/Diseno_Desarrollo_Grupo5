using System.Collections.Generic;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class CategoriaCrudVM
    {
        public int ID_CATEGORIA { get; set; }
        public string NOMBRE { get; set; }
        public string DESCRIPCION { get; set; }
        public int ID_ESTADO { get; set; }

        public List<CategoriaFilaVM> Categorias { get; set; }

        public List<SelectListItem> Estados { get; set; }
    }

    public class CategoriaFilaVM
    {
        public int ID_CATEGORIA { get; set; }
        public string NOMBRE { get; set; }
        public string DESCRIPCION { get; set; }

        public int ID_ESTADO { get; set; }
        public string ESTADO { get; set; }
    }
}
