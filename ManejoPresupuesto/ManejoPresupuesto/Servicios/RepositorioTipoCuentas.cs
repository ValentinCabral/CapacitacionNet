using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTipoCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int Id, int UsuarioId);
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
            var Id = await connection.QuerySingleAsync<int>(@"
                INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden)
                VALUES (@Nombre, @UsuarioId, 0);
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
            return await connection.QueryAsync<TipoCuenta>($"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId", new {usuarioId});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            // Execute se usa para querys que no retornan nada
            await connection.ExecuteAsync($"UPDATE TiposCuentas SET Nombre = @Nombre WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync($@"
                                                SELECT Id, Nombre,UsuarioId, Orden
                                                FROM TiposCuentas
                                                WHERE Id = {id} AND UsuarioId = {usuarioId}
                                                     ");
        }

    }
}
