using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly string connectionString;

        public TiposCuentasController(IConfiguration configuration, IRepositorioTipoCuentas repositorioTipoCuentas, IServicioUsuarios servicioUsuarios)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            RepositorioTipoCuentas = repositorioTipoCuentas;
            ServicioUsuarios = servicioUsuarios;
        }

        public IRepositorioTipoCuentas RepositorioTipoCuentas { get; }
        public IServicioUsuarios ServicioUsuarios { get; }

        // Accion que muestra el listado de tipos cuentas
        public async Task<IActionResult> Index()
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await RepositorioTipoCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.Id = ServicioUsuarios.ObtenerUsuarioId();
            // Controlo si ya existe un tipocuenta con ese nombre y ese id
            var yaExisteTipoCuenta = await RepositorioTipoCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (yaExisteTipoCuenta)
            {
                // Tiro un error 
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");
                return View(tipoCuenta);
            }   
            await RepositorioTipoCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await RepositorioTipoCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await RepositorioTipoCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);
            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado, Home");
            }
            await RepositorioTipoCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await RepositorioTipoCuentas.Existe(nombre, usuarioId);
            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await RepositorioTipoCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await RepositorioTipoCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await RepositorioTipoCuentas.Borrar(id);
            return RedirectToAction("Index");

        }
    }
}
