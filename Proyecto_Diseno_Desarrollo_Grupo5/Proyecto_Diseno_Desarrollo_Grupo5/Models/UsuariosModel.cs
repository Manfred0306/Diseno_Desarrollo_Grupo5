using System.ComponentModel.DataAnnotations;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class UsuariosModel
    {

        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar 80 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        [StringLength(120, ErrorMessage = "El correo no puede superar 120 caracteres")]
        public string Correo { get; set; }

        [StringLength(200, ErrorMessage = "La contraseña no puede superar 200 caracteres")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public int IdRol { get; set; }

        public int IdEstado { get; set; } // 1 Activo, 2 Inactivo

        public string ROL_NOMBRE { get; set; }
        public string ESTADO_NOMBRE { get; set; }
    }
}
