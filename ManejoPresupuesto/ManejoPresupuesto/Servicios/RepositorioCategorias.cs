﻿using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCategorias:IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //Crear una categoria
        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var Id = await connection.QuerySingleAsync<int>($@"
                INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                VALUES (@Nombre, @TipoOperacionid, @UsuarioId)
                SELECT SCOPE_IDENTITY()", categoria);
            categoria.Id = Id;
        }

        // Obtener todas las categorias del usuarioId
        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>($@"
                SELECT * FROM Categorias
                WHERE UsuarioId = @UsuarioId", new {usuarioId});
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>($@"
                SELECT * FROM Categorias
                WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @TipoOperacionId", new { usuarioId, tipoOperacionId});
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>($@"
                SELECT * FROM CATEGORIAS
                WHERE UsuarioId = @UsuarioId AND Id = @Id
            ", new {usuarioId, id});
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($@"
                UPDATE Categorias
                SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId
                WHERE Id = @Id
            ", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($@"
                DELETE Categorias
                WHERE Id = @Id
            ", new {id});
        }
    }
}
