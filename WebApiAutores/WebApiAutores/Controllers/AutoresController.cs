using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [Route("api/autores")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public AutoresController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        /*
         * Este método Get devuelve una lista de autores
         * La cual la trae desde la tabla Autores del DbContext
        */
        public async Task<ActionResult<List<Autor>>> Get()
        {
            return await context.Autores.ToListAsync();
        }

        [HttpPost]
        /*
         * Este método post toma un autor y lo agrega al DbContext
         * Luego guarda los cambios de manera asincrona
         * y devuelve un ok si esta todo correcto
        */
        public async Task<ActionResult> Post(Autor autor)
        {
            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")] // api/autores/Id
        /*
         * Lo que hace este metodo Put es actualizar un Autor
         * Toma como parametro el Id del autor que se quiere actualizar en la URL
         * Luego verifica que ese Id coincida con el ID del autor nuevo que se pasa, o que existe algun autor con ese Id
         * Si coincide, lo actualiza, guarda los cambios y retorna ok.
        */
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            if(autor.Id != id)
            {
                return BadRequest("El Id del autor que se quiere actualizar no coincide con el Id de la URL");
            }

            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();
        }


        [HttpDelete("{id:int}")] // api/autores/Id
        /*
         * Lo que hace este método Delete es
         * dado un Id en la URL, primero verifica que exista un autor con ese id
         * Luego si existe, lo remueve, guarda los cambios y retorna un ok
        */
        public async Task<ActionResult> Delete(int id)
        {
            // Algun autor con el Id == id
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                //No encontrado
                return NotFound();
            }

            // Borra el autor con Id = id
            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
