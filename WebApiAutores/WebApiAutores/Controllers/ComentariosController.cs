using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")]
    public class ComentariosController:ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        public ComentariosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get([FromRoute] int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var comentarios = await context.Comentarios.Where(x => x.LibroId == libroId).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id:int}", Name = "ObtenerComentarioId")]
        public async Task<ActionResult<ComentarioDTO>> GetPorId([FromRoute] int libroId, [FromRoute] int id)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentario is null)
                return NotFound();

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromRoute] int libroId,[FromBody] ComentarioCreacionDTO comentarioDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioDTO);
            comentario.LibroId = libroId;
            context.Add(comentario);
            await context.SaveChangesAsync();
            var comentarioDTORetorno = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("ObtenerComentarioId", new { libroId = libroId, id = comentario.Id }, comentarioDTORetorno);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int libroId,[FromRoute] int id,[FromBody] ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var existeComentario = await context.Comentarios.AnyAsync(x => x.Id == id);
            
            if (!existeComentario)
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;

            context.Update(comentario);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
