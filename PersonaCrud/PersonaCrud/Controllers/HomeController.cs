using Microsoft.AspNetCore.Mvc;
using PersonaCrud.Models;
using PersonaCrud.Servicios;
using System.Diagnostics;

namespace PersonaCrud.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepositorioPersonas repositorioPersonas;

        public HomeController(IRepositorioPersonas repositorioPersonas)
        {
            this.repositorioPersonas = repositorioPersonas;
        }

        public async Task<IActionResult> Index()
        {
            var personas = await repositorioPersonas.Obtener();
            return View(personas);
        }

        [HttpGet]
        public IActionResult Agregar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(Persona persona)
        {
            if (!ModelState.IsValid)
            {
                return View(persona);
            }

            await repositorioPersonas.Agregar(persona);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var persona = await repositorioPersonas.ObtenerPorId(id);
            if(persona is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(persona);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPersona(int id)
        {
            var persona = await repositorioPersonas.ObtenerPorId(id);
            
            if (persona is null)
            {
                RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioPersonas.Eliminar(id);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}