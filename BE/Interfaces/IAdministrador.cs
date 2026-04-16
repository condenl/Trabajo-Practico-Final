using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.Interfaces
{
    public interface IAdministrador
    {
        List<Usuario> Listar();
        void Insertar(Usuario usuario, string passwordSinHash);
        void Modificar(Usuario usuario, string nuevaPassword);
        void Eliminar(Usuario usuario);
    }
}