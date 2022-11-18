using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Tests
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributeTests
    {
        [TestMethod]
        // Con todos los strings que tengan la primer letra en minuscula el resultado deberia ser "La primera letra debe ser mayúscula"
        public void PrimeraLetraMinuscula_DevuelveError()
        {
            // Preparación
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var valores = new List<string>() {"valen", "cabri", "asdasd", "pasoLaPreuba", "yoTambienLaPaso", "laSigoPasando" };
            foreach (var valor in valores)
            {
                var valContext = new ValidationContext(new { Name = valor });

                // Ejecución
                var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);

                // Verificación
                Assert.AreEqual("La primera letra debe ser mayúscula", resultado.ErrorMessage);
            }
        }

        [TestMethod]
        // Todos los strings con la primer letra mayúscula deben pasar la prueba
        public void PrimeraLetraMayuscula_NoDevuelveError()
        {
            // Preparación
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var valores = new List<string>() { "Valen", "Cabri", "Aasdasd", "PasoLaPrueba", "YoTambienLaPaso", "LaSigoPasando" };
            foreach(var valor in valores)
            {
                var valContext = new ValidationContext(new {Name = valor});

                // Ejecución
                var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);

                // Verificación
                Assert.IsNull(resultado);
            }
        }

        [TestMethod]
        // Los string nulos deben pasar la preuba
        public void StringNulos_NoDevuelveError()
        {
            // Preparación
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            string valor = null;
            var valContext = new ValidationContext(new { Name = valor });

            // Ejecución
            var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);

            // Verificación
            Assert.IsNull(resultado);
        }
    }
}