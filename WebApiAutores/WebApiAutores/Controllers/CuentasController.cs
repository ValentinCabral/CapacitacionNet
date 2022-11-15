using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        private readonly SignInManager<IdentityUser> signInManager;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
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

                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors); // Si no se creo correctamente el usuario devuelvo los errores
            }

        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutentificacion>> Login(CredencialesUsuario credencialesUsuario)
        {

            // isPersistent y lockoutOnFailure son campos que no uso, por eso los pongo en false
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email, credencialesUsuario.Password, 
                isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded) // Si fue exitoso el logueo
                return await ConstruirToken(credencialesUsuario);  // Devuelvo el token de logueo

            return BadRequest("Login incorrecto");

        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutentificacion>> RenovarToken() {

            // Traigo el claim email del usuario
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            // Obtengo el valor de ese claim
            var email = emailClaim.Value;

            // Construyo las nuevas credenciales con el email del usuario que esta logueado
            var credencialUsuario = new CredencialesUsuario()
            {
                Email = email
            };

            // Construyo el JWT y lo retorno
            return await ConstruirToken(credencialUsuario);

        }

        [HttpPost("HacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            // Traigo el usuario que tenga ese email
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            usuario.UserName = usuario.Email;
            
            // Agrego el claim a ese usuario, el valor del claim puede ser cualquier cosa. En este caso "1"
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }

        [HttpPost("RemoverAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            // Traigo el usuario a partir de su email
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            usuario.UserName = usuario.Email;

            // Remuevo el claim a ese usuario
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }


        private async Task<RespuestaAutentificacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            // Un claim es informacion del usuario en el cual confiamos
            // No se puede devolver un password u informacion importante en un claim ya que no son secretos, el usuario los ve
            var claims = new List<Claim>()
            {
                // Son un par ordenado que tienen un campo y su valor
                new Claim("email", credencialesUsuario.Email)
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);

            var claimsDB = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsDB);

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
