using System.Collections.Generic;

namespace BE.Interfaces
{
    public interface IProveedorRepository
    {
        int Guardar(Proveedor proveedor);
        Proveedor Obtener(int idProveedor);
        IList<Proveedor> Listar(bool soloActivos);
    }
}
