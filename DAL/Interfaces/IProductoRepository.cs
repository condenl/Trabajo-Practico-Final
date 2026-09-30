using System.Collections.Generic;

using BE;

namespace DAL.Interfaces
{
    public interface IProductoRepository
    {
        int Guardar(Producto producto);
        Producto Obtener(int idProducto);
        IList<Producto> Listar(bool soloActivos);
    }
}
