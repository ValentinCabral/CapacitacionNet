using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs
{
    public class CredencialesUsuario
    {
        // Todo lo que el usuario necesita para autentificarse

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Password { get; set; }
    }
}
