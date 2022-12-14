using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [Route("api/libros")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }


        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<LibroDTO>>> Get()
        {
            // Lista de todos los libris
            var libros = await context.Libros
                .Include(x => x.Comentarios) // Incluyo los comentarios
                .Include(x => x.AutoresLibros) // Incluyo los autores
                .ThenInclude(autorLibro => autorLibro.Autor)
                .ToListAsync();


            foreach(var libro in libros)
            {
                // Por cada libro dentro de Libros ordeno sus autores por el campo Orden
                libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();  
            }

            return mapper.Map <List<LibroDTO>>(libros);
        }

        [HttpGet("{id:int}", Name = "ObtenerLibroId")] // api/libros/Id
        /*
         * Con este método Get traigo el primer libro que coincida
         * con el Id pasado por URL
         * A su vez, traigo los autores de ese libro.
        */
        public async Task<ActionResult<LibroDTO>> Get([FromRoute] int id)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == id); // Booleano
            if (!existeLibro) // No existe ninguno con ese Id
                return NotFound();


            // Libro con ese id
            var libro = await context.Libros
                .Include(x => x.Comentarios) // Incluyo los comentarios
                .Include(x => x.AutoresLibros) // Incluyo los autores
                .ThenInclude(autorLibro => autorLibro.Autor) 
                .FirstOrDefaultAsync(x => x.Id == id);

            // Ordeno los autores segun el campo Orden
            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<LibroDTO>(libro);
        }



        [HttpPost]
        /*
         * Este método Post recibe un Libro con sus autores
         * primero verifica que exista un autor con el AutorId de ese libro
         * Luego lo agrega al DbContext, guarda los cambios, y retorna un ok
        */
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds is null)  // Si no me pasan ningun id de autor
                return BadRequest("No se puede crear un libro sin autores");
            
            // Lista con los Ids de los autores que se encuentran contenidos los autoresIds de libroDTO
            var autoresIds = await context.Autores.Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id))
                .Select(x => x.Id) // Para seleccionar solamente el Id
                .ToListAsync();

            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count) // Si no recibi la misma cantidad de autores de los que obtuve
                return BadRequest("No existe uno de los autores enviados");

            var libro = mapper.Map<Libro>(libroCreacionDTO);  // Mapeo desde LibroCreacionDTO a libro (usando el mapeo especial)

            AsignarOrdenAutores(libro);
        

            context.Add(libro); // Agrego el libro
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD

            var libroDTORetorno = mapper.Map<LibroAutoresDTO>(libro);
            return CreatedAtRoute("ObtenerLibroId", new {id = libro.Id}, libroDTORetorno);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libro = await context.Libros
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
                return NotFound();

            /*
             * Llevo las propiedades de libroCreacionDTO hacia libro
             * por lo que actualizo libro
             * y asigno el resultado en libro
            */

            libro = mapper.Map(libroCreacionDTO, libro);

            AsignarOrdenAutores(libro);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch([FromRoute] int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument is null)
                return BadRequest();

            var libro = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
                return NotFound();

            var libroDTO = mapper.Map<LibroPatchDTO>(libro);


            // Aplico los cambios que vienen desde el patchDocument en libroDTO
            patchDocument.ApplyTo(libroDTO, ModelState); // Paso el ModelState para que se guarde ahi en caso de que existe algún error

            var esValido = TryValidateModel(libroDTO); // Para verificar que todo lo que quedo en libroDTO es válido

            if (!esValido)
                return BadRequest(ModelState);

            // Hago los cambios
            mapper.Map(libroDTO, libro);

            await context.SaveChangesAsync(); // Persisto y guardo en la BD
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == id);

            if (!existeLibro)
                return NotFound();

            context.Remove(new Libro() { Id = id});
            await context.SaveChangesAsync();
            return NoContent();
        }

        /*
         * Ordena los autores del libro segun el campo orden
        */
        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null) // Si tiene elementos
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i; // Los ordeno 
                }
            }
        }
    }
}
