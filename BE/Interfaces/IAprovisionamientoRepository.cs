using System;
using System.Collections.Generic;

namespace BE.Interfaces
{
    public interface IAprovisionamientoRepository
    {
        int InsertarCotizacion(CotizacionProveedor cotizacion);
        int InsertarOrden(OrdenCompra orden);
        OrdenCompra ObtenerOrden(int idOrdenCompra);
        bool ActualizarEstado(int idOrdenCompra, EstadoOrdenCompra estadoEsperado, EstadoOrdenCompra nuevoEstado, int? idUsuarioContable, string motivoRechazo);
        IDictionary<int, int> ContarProveedoresCotizadosPorProducto(IList<int> idsProducto, DateTime fechaReferencia);
        IList<OrdenCompra> ListarOrdenes(int? idProveedor);
        void RegistrarRecepcion(int idOrdenCompra, IList<Lote> lotes, EstadoOrdenCompra estadoFinal, string responsable);
    }
}
