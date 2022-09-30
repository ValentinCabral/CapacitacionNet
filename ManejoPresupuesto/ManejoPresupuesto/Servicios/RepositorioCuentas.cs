﻿using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Crear(Cuenta cuenta);
    }
    public class RepositorioCuentas:IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var Id = await connection.QuerySingleAsync<int>(@"
                INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance)
                VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance)
                SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = Id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>($@"
                        SELECT Cuentas.Id, Cuentas.Nombre, Cuentas.Balance, tc.Nombre AS TipoCuenta 
                        FROM Cuentas
                        INNER JOIN TiposCuentas tc
                        ON tc.Id = Cuentas.TipoCuentaId
                        WHERE tc.UsuarioId = @UsuarioId
                        ORDER BY tc.Orden", new { usuarioId });
        }
    }
}