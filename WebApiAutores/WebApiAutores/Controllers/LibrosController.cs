using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [Route("api/libros")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public LibrosController(ApplicationDbContext context)
        {
            this.context = context;
        }



        [HttpGet("{id:int}")] // api/libros/Id
        /*
         * Con este método Get traigo el primer libro que coincida
         * con el Id pasado por URL
         * A su vez, traigo el Autor de ese libro.
        */
        public async Task<ActionResult<Libro>> Get([FromRoute] int id)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == id);
            if (!existeLibro)
                return NotFound();
            // Con el include traigo el autor de ese libro
            return await context.Libros.Include(x => x.Autor).FirstOrDefaultAsync(x => x.Id == id);
        }



        [HttpPost]
        /*
         * Este método Post recibe un Libro
         * primero verifica que exista un autor con el AutorId de ese libro
         * Luego lo agrega al DbContext, guarda los cambios, y retorna un ok
        */
        public async Task<ActionResult> Post([FromBody] Libro libro)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId);
            if (!existeAutor)
                return BadRequest($"No existe el autor de Id: {libro.AutorId}");

            context.Add(libro);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
