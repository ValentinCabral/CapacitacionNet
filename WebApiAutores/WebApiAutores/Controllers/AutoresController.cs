using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [Route("api/autores")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutoresController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        /*
         * Este método Get devuelve una lista de autores
         * La cual la trae desde la tabla Autores del DbContext
        */
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            // Obtengo todos los tipo <Autor>
            var autores =  await context.Autores
                .Include(x => x.AutoresLibros) // Incluyo los libros del Autor
                .ThenInclude(autorLibro => autorLibro.Libro)
                .ToListAsync();  // A una lista

            return mapper.Map<List<AutorDTO>>(autores); // Mapeo los autores desde <Autor> a <AutorDTO> y retorno
        }


        [HttpGet("{id:int}", Name = "obtenerAutorId")]
        [AllowAnonymous]
        /*
         * Devuelve un autor segun su id
        */
        public async Task<ActionResult<AutorDTO>> GetId([FromRoute] int id)
        {
            var autor = await context.Autores
                .Include(x => x.AutoresLibros) // Incluyo los libros del autor
                .ThenInclude(autorLibro => autorLibro.Libro)
                .FirstOrDefaultAsync(x => x.Id == id); // Primero que encuentre

            if (autor is null) //No encontro ninguno
                return NotFound();

            return mapper.Map<AutorDTO>(autor); // Mapeo el <Autor> hacia <AutorDTO>
        }

        [HttpGet("{nombre}")]
        [AllowAnonymous]
        /*
         * Devuelve un autor segun su nombre
         * Si contiene el string tambien lo devuelve
         * Ejemplo Autor: Valentin Cabral
         * Si paso "Valen" lo devuelve
        */
        public async Task<ActionResult<AutorDTO>> GetNombre([FromRoute] string nombre)
        {
            var autor = await context.Autores
                .Include(x => x.AutoresLibros) // Incluyo los libros del autor
                .ThenInclude(autorLibro => autorLibro.Libro)
                .FirstOrDefaultAsync(x => x.Nombre.Contains(nombre)); //Primero que encuentre

            if (autor is null) //No encontro ninguno
                return NotFound();

            return mapper.Map<AutorDTO>(autor); // Mapeo el <Autor> hacia <AutorDTO>
        }

        [HttpGet("listado/{nombre}")]
        [AllowAnonymous]
        /*
         * Devuelve la lista de todos los autores que contengan el string
        */
        public async Task<ActionResult<List<AutorDTO>>> GetNombres([FromRoute] string nombre)
        {
            var autores = await context.Autores
                .Include(x => x.AutoresLibros) // Incluyo los libros de los autores
                .ThenInclude(autorLibro => autorLibro.Libro)
                .ToListAsync(); //Lista con todos los autores
            autores.RemoveAll(x => !x.Nombre.Contains(nombre)); //Remuevo los autores que no tengan el nombre dentro

            if(autores.Count() == 0) //Ninguno tiene el nombre
                return NotFound();

            return mapper.Map<List<AutorDTO>>(autores); // Mapeo los autores desde <Autor> a <AutorDTO> y retorno
        }

        [HttpPost]
        /*
         * Este método post toma un autor y lo agrega al DbContext
         * Luego guarda los cambios de manera asincrona
         * y devuelve un ok si esta todo correcto
        */
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorDTO)
        {
            var yaExisteAutor = await context.Autores.AnyAsync(x => x.Nombre == autorDTO.Nombre); // Booleano

            if (yaExisteAutor) //Ya existe un autor con ese nombre
                return BadRequest("Ya existe un autor con ese nombre");


            //Mapeo del autorDTO a un autor
            var autor = mapper.Map<Autor>(autorDTO);

            context.Add(autor); // Agrego el autor
            await context.SaveChangesAsync(); //Persisto los datos en la BD

            var autorRetornoDTO = mapper.Map<AutorDTO>(autor);  // Creo un nuevo AutorDTO para retornarlo en la ruta
            return CreatedAtRoute("obtenerAutorId",new {id = autor.Id },  autorRetornoDTO); // Nombre de la ruta, parametro de esa ruta, y el objeto
        }

        [HttpPut("{id:int}")] // api/autores/Id
        /*
         * Lo que hace este metodo Put es actualizar un Autor
         * Toma como parametro el Id del autor que se quiere actualizar en la URL
         * Luego verifica que ese Id coincida con el ID del autor nuevo que se pasa, o que existe algun autor con ese Id
         * Si coincide, lo actualiza, guarda los cambios y retorna ok.
        */
        public async Task<ActionResult> Put([FromBody] AutorCreacionDTO autorDTO,[FromRoute] int id)
        {

            var existe = await context.Autores.AnyAsync(x => x.Id == id); // Booleano

            if (!existe) //No existe ese autor
                return NotFound();

            // Mapeo el dto a autor
            var autor = mapper.Map<Autor>(autorDTO);
            autor.Id = id;

            context.Update(autor); //Agrego el autor
            await context.SaveChangesAsync(); //Persisto los datos en la BD
            return Ok();
        }


        [HttpDelete("{id:int}")] // api/autores/Id
        /*
         * Lo que hace este método Delete es
         * dado un Id en la URL, primero verifica que exista un autor con ese id
         * Luego si existe, lo remueve, guarda los cambios y retorna un ok
        */
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            // Algun autor con el Id == id
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
                //No encontrado
                return NotFound();

            // Borra el autor con Id = id
            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
