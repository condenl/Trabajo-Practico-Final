using System.Collections.Generic;

namespace BE.Interfaces
{
    public interface IProductoRepository
    {
        int Guardar(Producto producto);
        Producto Obtener(int idProducto);
        IList<Producto> Listar(bool soloActivos);
    }
}
