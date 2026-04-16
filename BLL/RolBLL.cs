using System;
using System.Collections.Generic;
using BE;
using BE.Interfaces;
using DAL;

namespace BLL
{
    public class RolBLL : IGestor<Rol>
    {
        public RolBLL() { }
        public List<Rol> ListarTodos()
        {
            return RolDAL.Instancia.Listar();
        }

        public void Guardar(Rol rol)
        {
            if (string.IsNullOrWhiteSpace(rol.Nombre))
                throw new Exception("El nombre del rol es obligatorio.");

            if (string.IsNullOrWhiteSpace(rol.Descripcion))
                throw new Exception("La descripción del rol es obligatoria.");

            if (rol.Id == 0)
            {
                RolDAL.Instancia.Insertar(rol);
            }
            else
            {
                RolDAL.Instancia.Modificar(rol);
            }
        }

        public void Eliminar(int id)
        {
            if (id <= 0) throw new Exception("ID de rol inválido.");
            RolDAL.Instancia.Eliminar(id);
        }

        public List<Rol> Listar()
        {
            return ListarTodos();
        }

        public int Modificar(Rol rol)
        {
            Guardar(rol);
            return 1;
        }
    }
}