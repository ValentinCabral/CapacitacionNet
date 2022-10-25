using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Servicios
{
    public class AutoMapperProfile:Profile
    {

        public AutoMapperProfile()
        {
            //Autores 
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<AutorActualizacionDTO, Autor>();
            CreateMap<Autor, AutorActualizacionDTO>()
                .ForMember(autorActualizacionDTO => autorActualizacionDTO.Libros, opciones => opciones.MapFrom(MapAutorDTOLibros));
            
            // Libros
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));
            CreateMap<Libro, LibroDTO>()
                .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));
            CreateMap<Libro, LibroAutoresDTO>();
            CreateMap<LibroAutoresDTO, Libro>();

            // Comentarios
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
        }

        // Me devuelve una lista de AutorLibro
        private List<AutorLibro> MapAutoresLibros (LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            var resultado = new List<AutorLibro>(); // El resultado que voy a devolver

            if (libroCreacionDTO.AutoresIds is null)  // Si no tiene ningún autor retorno el resultado vacio
                return resultado;

            foreach(var autorId in libroCreacionDTO.AutoresIds)  // Para aca id de un autor en el libro recibido
            {
                resultado.Add(new AutorLibro { AutorId = autorId }); // Agrego uno nuevo al resultado asignando ese id
            }

            return resultado;
        }


        //Devuelvo una lista de AutorActualizacionDTO (Id, Nombre)
        private List<AutorActualizacionDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorActualizacionDTO>(); // El resultado que voy a devolver

            if (libro.AutoresLibros is null) // Si no tengo ningún autor devuelvo el resultado vacio
                return resultado;

            foreach(var autorLibro in libro.AutoresLibros) // Por cada autorLibro en AutoresLibros
            {
                resultado.Add(new AutorActualizacionDTO { Id = autorLibro.AutorId, Nombre = autorLibro.Autor.Nombre }); // Creo un nuevo autor y asigno su id y nombre
            }

            return resultado;
        }

        private List<LibroAutoresDTO> MapAutorDTOLibros(Autor autor, AutorActualizacionDTO autorActualizacionDTO)
        {
            var resultado = new List<LibroAutoresDTO>(); // El resultado que voy a devolver

            if (autor.AutoresLibros is null) // Si no tengo ningún autor devuelvo el resultado vacio
                return resultado;

            foreach(var autorLibro in autor.AutoresLibros) // Por cada autorLibro en AutoresLibros
            {
                // Creo un nuevo Libro y asigno su id y titulo
                resultado.Add(new LibroAutoresDTO()
                {
                        Id = autorLibro.LibroId, 
                        Titulo = autorLibro.Libro.Titulo
                });
            }

            return resultado;
        }
    }
}
