using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController :ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [HttpPost("registrar")] // api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutentificacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            // Creo un nuevo usuario
            var usuario = new IdentityUser { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email };
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);

            if (resultado.Succeeded) // Si el usuario se creo correctamente
            {
                // Retorno el JWT, es decir el token que se le devuelve al cliente para que puedan autentificarse con el mismo

                return ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors); // Si no se creo correctamente el usuario devuelvo los errores
            }

        }

        private RespuestaAutentificacion ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            // Un claim es informacion del usuario en el cual confiamos
            // No se puede devolver un password u informacion importante en un claim ya que no son secretos, el usuario los ve
            var claims = new List<Claim>()
            {
                // Son un par ordenado que tienen un campo y su valor
                new Claim("email", credencialesUsuario.Email)
            };


            // Construyo el token
            // Traigo la llave desde appsetings usando configuration
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["LlaveJWT"]));
            // Paso la llave y el algoritmo de seguridad que quiero usar
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);


            // Hago que la llave expire en 1 año
            var expiracion = DateTime.UtcNow.AddYears(1);

            // Genero el token
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);


            // Retorno la nueva respuesta autentificacion
            return new RespuestaAutentificacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }
    }
}
