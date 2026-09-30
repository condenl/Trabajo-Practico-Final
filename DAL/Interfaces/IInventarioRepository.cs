using System.Collections.Generic;

using BE;

namespace DAL.Interfaces
{
    public interface IInventarioRepository
    {
        int VerificarStock(int idProducto);
        int RegistrarIngresoLote(Lote lote, string responsable, string referencia);
        bool DescontarStockLote(int idLote, int cantidad, string responsable, string referencia);
        IList<Lote> ObtenerLotesDisponibles(int idProducto);
        IList<Lote> ObtenerTrazabilidad(int idProducto);
    }
}
