using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Servicios
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<AutorActualizacionDTO, Autor>();
            CreateMap<LibroCreacionDTO, Libro>();
            CreateMap<Autor, AutorActualizacionDTO>();
        }
    }
}
