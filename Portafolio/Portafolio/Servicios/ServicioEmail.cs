using Portafolio.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Portafolio.Servicios
{
    public interface IServicioEmail { 
    }
    public class ServicioEmail:IServicioEmail
    {
        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
            Configuration = configuration;
        }

        public async Task enviar(ContactoViewModel contacto)
        {
            var apiKey = Configuration.GetValue<string>("SENDGRID_API_KEY");
            var email = Configuration.GetValue<string>("SENDGRID_FROM");
            var nombre = Configuration.GetValue<string>("SENDGRID_NOMBRE");

            var cliente = new SendGridClient(apiKey);
            var from = new EmailAddress(email, nombre);
            var subject = $"El cliente {contacto.Email} quiere contactarte";
            var to = new EmailAddress(email, nombre);
            var mensajeTextoPlano = contacto.Mensaje;
            var contenidoHtml = $@""
        }

        public IConfiguration Configuration { get; }
    }
}
