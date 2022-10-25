using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class LibroCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula(ErrorMessage = "La primer letra del campo {0} debe ser una mayúscula")]
        public string Titulo { get; set; }
        [Required(ErrorMessage = "El campo {0 es requerido}")]
        public List<int> AutoresIds { get; set; }
    }
}
