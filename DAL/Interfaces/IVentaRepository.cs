using System.Collections.Generic;

using BE;

namespace DAL.Interfaces
{
    public interface IVentaRepository
    {
        Cliente BuscarClientePorEmail(string email);
        int GuardarCliente(Cliente cliente);
        Membresia ObtenerMembresiaActiva(int idCliente);
        int CrearMembresia(Membresia membresia);
        int RegistrarVenta(Venta venta, string responsable);
        IList<Venta> ListarVentasCliente(int idCliente);
    }
}
