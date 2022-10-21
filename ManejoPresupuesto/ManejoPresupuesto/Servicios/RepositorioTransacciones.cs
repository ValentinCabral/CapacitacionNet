using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal MontoAnterior, int CuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones:IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var Id = await connection.QuerySingleAsync<int>($@"
                    INSERT INTO Transacciones (UsuarioId, FechaTransaccion, Monto, Nota, CuentaId, CategoriaId)
                    VALUES (@UsuarioId, @FechaTransaccion, ABS(@Monto), @Nota, @CuentaId, @CategoriaId)

                    UPDATE Cuentas
                    SET Balance += @Monto
                    WHERE Id = @CuentaId

                    SELECT SCOPE_IDENTITY();", transaccion);
            transaccion.Id = Id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal MontoAnterior, int CuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($@"
                    -- Revertir la transacción anterior
                    UPDATE Cuentas
                    SET Balance -= @MontoAnterior
                    WHERE Id = @CuentaAnteriorId;

                    --Realizar la nueva transacción
                    UPDATE Cuentas
                    SET Balance += @Monto
                    WHERE Id = @CuentaId;

                    UPDATE Transacciones
                    SET Monto = ABS(@Monto), FechaTransaccion = @FechaTransaccion,
                    CategoriaId = @CategoriaId, CuentaId = @CuentaId, Nota = @Nota
                    WHERE Id = @Id",new
                    {
                        MontoAnterior,
                        CuentaAnteriorId,
                        transaccion.Id,
                        transaccion.FechaTransaccion,
                        transaccion.Monto,
                        transaccion.CategoriaId,
                        transaccion.CuentaId,
                        transaccion.Nota,
                        transaccion.UsuarioId,
                        
                    });
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>($@"
                    SELECT Transacciones.*, cat.TipoOperacionId 
                    FROM Transacciones
                    INNER JOIN Categorias cat
                    ON cat.Id = Transacciones.CategoriaId
                    WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId", new {id, usuarioId});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync($@"Transacciones_Borrar", new {id}, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>($@"
                    SELECT DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 AS Semana, 
                    SUM(Monto) AS Monto, cat.TipoOperacionId
                    FROM Transacciones
                    INNER JOIN Categorias cat
                    ON cat.Id = Transacciones.CategoriaId
                    WHERE Transacciones.UsuarioId = @usuarioId  
                    AND FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
                    GROUP BY DATEDIFF(D, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>($@"
                    SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre AS Categoria,
                    cu.Nombre AS Cuenta, c.TipoOperacionId 
                    FROM Transacciones t
                    INNER JOIN Categorias c
                    ON c.Id = t.CategoriaId
                    INNER JOIN Cuentas cu
                    ON cu.Id = t.CuentaId
                    WHERE t.UsuarioId = @UsuarioId AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                    ORDER BY t.FechaTransaccion DESC
            ", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>($@"
                    SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre AS Categoria,
                    cu.Nombre AS Cuenta, c.TipoOperacionId 
                    FROM Transacciones t
                    INNER JOIN Categorias c
                    ON c.Id = t.CategoriaId
                    INNER JOIN Cuentas cu
                    ON cu.Id = t.CuentaId
                    WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin

            ", modelo);
        }
    }
}
