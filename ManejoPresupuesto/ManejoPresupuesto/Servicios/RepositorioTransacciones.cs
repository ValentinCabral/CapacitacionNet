using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
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
    }
}
