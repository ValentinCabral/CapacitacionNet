using System.ComponentModel.DataAnnotations;
using WebApiAutores.Entidades;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class LibroDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula(ErrorMessage = "La primer letra del campo {0} debe ser una mayúscula")]
        public string Titulo { get; set; }
        public Autor Autor { get; set; }
    }
}
