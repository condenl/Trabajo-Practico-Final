using System;
using System.Collections.Generic;
using BE;

namespace DAL.Interfaces
{
    public interface IEvaluacionProveedorRepository
    {
        DatosEvaluacion ObtenerDatos(int idProducto, DateTime fechaCorte);
        int GuardarEvaluacion(EvaluacionProveedor evaluacion);
        EvaluacionProveedor ObtenerEvaluacion(int idEvaluacion);
        IList<ResumenEvaluacion> ListarHistorial(int idProducto);
        void SeleccionarPrioritario(int idEvaluacion, int idProveedor, string motivo, string responsable);
        int CrearOrdenBorrador(int idEvaluacion, string responsable);
    }
}
