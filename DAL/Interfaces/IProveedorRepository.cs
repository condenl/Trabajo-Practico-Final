using System.Collections.Generic;

using BE;

namespace DAL.Interfaces
{
    public interface IProveedorRepository
    {
        int Guardar(Proveedor proveedor);
        Proveedor Obtener(int idProveedor);
        IList<Proveedor> Listar(bool soloActivos);
    }
}
