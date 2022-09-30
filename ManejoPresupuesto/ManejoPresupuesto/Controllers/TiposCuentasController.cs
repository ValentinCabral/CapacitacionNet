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

        /* 
         * Accion para crear un nuevo elemento en la tabla de tipos cuentas
         * Solo la vista
         */
        public IActionResult Crear()
        {
            return View();
        }


        [HttpPost]
        /*
         * Accion que recibe un nuevo elemento tipoCuenta desde un formulario
         * y lo agrega en la tabla.
         * A su vez si el tipoCuenta ya existe tira un error, y tiene validaciones
         */
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
        /*
         * Accion que me lleva a la vista de edicion de un elemento de la tabla
         * Si el elemento no fue encontrado me lleva a la vista de un error
         */
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
        /*
         * Accion que me permite editar un elemento tipoCuenta de la tabla
         * gracias a un formulario
         * Tiene validaciones por si el tipoCuenta no existe.
         */
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
        /*
         * Accion que dado un nombre de un tipoCuenta me dice si ya existe o no
         */
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

        /*
         * Accion que me lleva a la vista para borrar un tipoCuenta
         * Si el mismo no existe me da un error
         */
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
        /*
         * Accion que borra el tipoCuenta
         */
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

        [HttpPost]
        /*
         * Accion que recibe un arreglo de ids desde el cuerpo de la vista
        */

        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            // Obtengo todos los tipoCuenta de ese usuarioId
            var tiposCuentas = await RepositorioTipoCuentas.Obtener(usuarioId);

            // Guardo los ids de los tipos cuentas que tengo en tiposCuentas
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if(idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) =>
            new TipoCuenta() { Id = valor, Orden = indice+1}).AsEnumerable();

            await RepositorioTipoCuentas.Ordenar(tiposCuentasOrdenados);
            return Ok();
        } 
    }
}
