using System;

namespace BE.Interfaces
{
    public interface IObserver
    {
        void Actualizar(EventoBitacora alerta);
    }
}