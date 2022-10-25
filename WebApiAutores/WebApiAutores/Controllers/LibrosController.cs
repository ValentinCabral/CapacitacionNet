using AutoMapper;
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

        [HttpGet("{id:int}")] // api/libros/Id
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

            if(libro.AutoresLibros != null) // Si tiene elementos
            {
                for(int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i; // Los ordeno 
                }
            }

            context.Add(libro); // Agrego el libro
            await context.SaveChangesAsync(); // Persisto y guardo los cambios en la BD
            return Ok();
        }
    }
}
