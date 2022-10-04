using System.ComponentModel.DataAnnotations;

namespace PersonaCrud.Models
{
    public class Persona
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} no puede estar vacío")]
        [StringLength(maximumLength:50, ErrorMessage = "El campo {0} puede tener un máximo de 50 caracteres")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El campo {0} no puede estar vacío")]
        [StringLength(maximumLength: 50, ErrorMessage = "El campo {0} puede tener un máximo de 50 caracteres")]
        public string Apellido { get; set; }
    }
}
