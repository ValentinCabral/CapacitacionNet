using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
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

        // Esto va a hacer que este endpoind necesite autentificacion, y también me permite acceder a los claims del usuario
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromRoute] int libroId,[FromBody] ComentarioCreacionDTO comentarioDTO)
        {

            // Obtengo el claim del email que viene desde HttpContext
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            // Obtengo el valor del claim de email
            var email = emailClaim.Value;

            // Obtengo el usuario
            var usuario = await userManager.FindByEmailAsync(email);

            // Obtengo el Id del usuario
            var usuarioId = usuario.Id;

            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioDTO);
            comentario.LibroId = libroId;
            // Agrego el Id del usuario
            comentario.UsuarioId = usuarioId;

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
