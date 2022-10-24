using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Libro
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula(ErrorMessage = "La primer letra del campo {0} debe ser una mayúscula")]
        public string Titulo { get; set; }
        [Required(ErrorMessage = "El campo {0 es requerido}")]
        public int AutorId { get; set; }
        public Autor Autor { get; set; }
        public List<Comentario> Comentarios { get; set; }
    }
}
