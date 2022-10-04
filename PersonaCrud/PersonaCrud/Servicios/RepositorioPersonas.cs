using Dapper;
using Microsoft.Data.SqlClient;
using PersonaCrud.Models;

namespace PersonaCrud.Servicios
{
    public interface IRepositorioPersonas
    {
        Task Agregar(Persona persona);
        Task Eliminar(int id);
        Task<IEnumerable<Persona>> Obtener();
        Task<Persona> ObtenerPorId(int id);
    }
    public class RepositorioPersonas:IRepositorioPersonas
    {
        private readonly string connectionString;
        public RepositorioPersonas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Obtener todas las personas
        public async Task<IEnumerable<Persona>> Obtener()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Persona>($@"SELECT * FROM Persona");
        }

        public async Task Agregar(Persona persona)
        {
            using var connection = new SqlConnection(connectionString);
            var Id =  await connection.QuerySingleAsync<int>($@"
                            INSERT INTO Persona (Nombre, Apellido)
                            VALUES (@Nombre, @Apellido)
                            SELECT SCOPE_IDENTITY();",persona);
            persona.Id = Id;
        }

        // Me busca una persona segun su id
        public async Task<Persona> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Persona>($@"
                            SELECT * FROM Persona
                            WHERE Id = @Id", new {id});
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($@"
                DELETE Persona
                WHERE Id = @Id", new {id});
        }
    }
}
