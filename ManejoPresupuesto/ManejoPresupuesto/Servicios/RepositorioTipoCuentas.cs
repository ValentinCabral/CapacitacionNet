using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTipoCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int Id, int UsuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }
    public class RepositorioTipoCuentas : IRepositorioTipoCuentas
    {
        // String que va a contener la cadena de conexion
        private readonly string connectionString;
        // Uso un iconfiguration para poder acceder al json donde esta la cadena de conexion
        public RepositorioTipoCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            // La cantidad de elementos que hay en la tabla de SQL
            var cantidadDeElementos = await connection.QuerySingleAsync<int>($@"SELECT COUNT (*) FROM TiposCuentas");
            if(cantidadDeElementos == 0)
            {
                // Si no tiene ninguno entonces el orden tiene que arrancar por 1
                tipoCuenta.Orden = 1;
            }else 
            {
                /*
                 Pero si tiene elementos, selecciono el Orden del elemento que tenga el maximo orden y le sumo uno
                 Ya que se supone que los elementos de la tabla deberian estar ordenados en orden.
                 */
                tipoCuenta.Orden = await connection.QuerySingleAsync<int>($@"SELECT Orden FROM TiposCuentas WHERE Orden = (SELECT MAX(Orden) FROM TiposCuentas)") + 1;
            }
            // Creo el nuevo elemento en la tabla con el nombre, usuario id y orden especificados, y guardo su ID.
            var Id = await connection.QuerySingleAsync<int>(@"
                INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden)
                VALUES (@Nombre, @UsuarioId, @Orden);
                SELECT SCOPE_IDENTITY();", tipoCuenta
                );
            tipoCuenta.Id = Id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            // Creo la variable de conexion
            using var connection = new SqlConnection(connectionString);
            // Devuelve el primero que encuentra y en caso de que no encuentre nada devuelve el valor por defecto de int (0)
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                                        @"
                                            SELECT 1 FROM TiposCuentas
                                            WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId
                                        ", new { nombre, usuarioId });
            return existe == 1;
        }

        // Metodo que permite obtener los tipos cuentas que coincidan con usuarioId
        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@$"
                            SELECT Id, Nombre, Orden 
                            FROM TiposCuentas 
                            WHERE UsuarioId = @UsuarioId
                            ORDER BY Orden", new {usuarioId});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta) {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($@"
                                             UPDATE TiposCuentas
                                             SET Nombre = @Nombre
                                             WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>($@"
                                                                SELECT Id, Nombre, Orden
                                                                FROM TiposCuentas
                                                                WHERE Id = @Id AND UsuarioId = @UsuarioId", new {id, usuarioId});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($@"
                                            DELETE TiposCuentas
                                            WHERE Id = @Id", new {id});
        }

        /*
         * Dado un enumerable de tiposCuentas
         * Actualizo ordenando los ordenes
         */
        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id";
            using var connection = new SqlConnection(connectionString);
            // Ejecuta el query en cada tipoCuenta que este en tipoCuentasOrdenados
            await connection.ExecuteAsync(query, tipoCuentasOrdenados);
        }

    }
}
