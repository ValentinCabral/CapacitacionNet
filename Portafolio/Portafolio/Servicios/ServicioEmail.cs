using Portafolio.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Portafolio.Servicios
{
    public interface IServicioEmail
    {
        Task Enviar(ContactoViewModel contacto);
    }
    public class ServicioEmail: IServicioEmail
    {
        // Uso IConfiguration por que necesito colocar la llave de sengrid en un proovedor de configuracion
        private readonly IConfiguration configuration;
        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task Enviar(ContactoViewModel contacto)
        {
            // Obtengo la clave
            var apiKey = configuration.GetValue<String>("SENGRID_API_KEY");
            // Obtengo el email desde (from) el que se envia
            var email = configuration.GetValue<String>("SENGRID_FROM");
            // Obtengo el nombre de quien envia el email
            var name = configuration.GetValue<String>("SENGRID_NOMBRE");

            // Cliente con la apikey correspondiente
            var cliente = new SendGridClient(apiKey);
            // Desde donde
            var from = new EmailAddress(email);
            var subject = $"El cliente {contacto.Email} quiere contactarte.";
            // Hacia donde
            var to = new EmailAddress(email, name);
            var mensajeTextoPlano = contacto.Mensaje;
            // Creo el mensaje pero en formato html
            var contenidoHTML = $@"De: {contacto.Nombre} -
            Email: {contacto.Email}
            Mensaje: {contacto.Mensaje}
            ";
            // Creo el email
            var singleEmail = MailHelper.CreateSingleEmail(from, to, subject, mensajeTextoPlano, contenidoHTML);
            // Envio el email
            var respuesta = await cliente.SendEmailAsync(singleEmail);
        }
    }
}
