using Microsoft.AspNetCore.Mvc;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]         // Para automatizar validaciones 
    [Route("api/autores")]  //Ruta para acceder al controlador
    public class AutoresController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<Autor>> Get()
        {
            return new List<Autor>()
            {
                new Autor() {Id = 1, Nombre = "Valentin"},
                new Autor() {Id = 2, Nombre = "Tomas"}
            };
        }
    }
}
