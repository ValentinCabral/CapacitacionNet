namespace WebApiAutores.DTOs
{
    public class RespuestaAutentificacion
    {
        public string Token { get; set; }
        public DateTime Expiracion { get; set; }
    }
}
