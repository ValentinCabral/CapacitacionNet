namespace WebApiAutores.Middlewares
{
    public class LoguearRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate siguiente;

        public LoguearRespuestaHTTPMiddleware(RequestDelegate siguiente)
        {
            this.siguiente = siguiente;
        }

        // Invoke o InvokeAsync
        public async Task InvokeAsync(HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                // Guardo la respuesta actual
                var cuerpoOriginalRespuesta = contexto.Response.Body;

                // Actualizo la respuesta por el memory stream
                contexto.Response.Body = ms;

                // Invoco al siguiente middleware
                await siguiente(contexto);

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginalRespuesta);
                contexto.Response.Body = cuerpoOriginalRespuesta;


                
                // Me devuelve en la consola la respuesta de cada petición
                Console.WriteLine(respuesta);
            }
        }
    }
}
