using System;
using System.Collections.Generic;

using BE;

namespace DAL.Interfaces
{
    public interface IAprovisionamientoRepository
    {
        int InsertarCotizacion(CotizacionProveedor cotizacion);
        int InsertarOrden(OrdenCompra orden);
        IList<int> InsertarOrdenes(IList<OrdenCompra> ordenes);
        OrdenCompra ObtenerOrden(int idOrdenCompra);
        bool ActualizarEstado(int idOrdenCompra, EstadoOrdenCompra estadoEsperado, EstadoOrdenCompra nuevoEstado, int? idUsuarioContable, string motivoRechazo);
        IDictionary<int, int> ContarProveedoresCotizadosPorProducto(IList<int> idsProducto, DateTime fechaReferencia);
        IList<OrdenCompra> ListarOrdenes(int? idProveedor);
        void RegistrarRecepcion(int idOrdenCompra, IList<Lote> lotes, EstadoOrdenCompra estadoFinal, string responsable);
    }
}
