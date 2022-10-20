using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres")]
        [PrimeraLetraMayuscula(ErrorMessage = "La primer letra del campo {0} debe ser una mayúscula")]
        public string Nombre { get; set; }
        public List<Libro> Libros { get; set; } 
    }
}
