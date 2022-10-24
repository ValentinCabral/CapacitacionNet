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
            var libros = await context.Libros.Include(x => x.Autor).Include(x => x.Comentarios).ToListAsync();

            return mapper.Map <List<LibroDTO>>(libros);
        }

        [HttpGet("{id:int}")] // api/libros/Id
        /*
         * Con este método Get traigo el primer libro que coincida
         * con el Id pasado por URL
         * A su vez, traigo el Autor de ese libro.
        */
        public async Task<ActionResult<LibroDTO>> Get([FromRoute] int id)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == id); // Booleano
            if (!existeLibro) // No existe ninguno con ese Id
                return NotFound();


            // Con el include traigo el autor de ese libro
            var libro = await context.Libros.Include(x => x.Autor).FirstOrDefaultAsync(x => x.Id == id); // Libro con ese id
            
            return mapper.Map<LibroDTO>(libro);
        }



        [HttpPost]
        /*
         * Este método Post recibe un Libro
         * primero verifica que exista un autor con el AutorId de ese libro
         * Luego lo agrega al DbContext, guarda los cambios, y retorna un ok
        */
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroDTO)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.Id == libroDTO.AutorId); // Booleano
            if (!existeAutor)
                return BadRequest($"No existe el autor de Id: {libroDTO.AutorId}");

            //Mapeo el dto a libro
            var libro = mapper.Map<Libro>(libroDTO);

            context.Add(libro); //Agrego el libro
            await context.SaveChangesAsync(); //Persisto la data en la DB
            return Ok();
        }
    }
}
