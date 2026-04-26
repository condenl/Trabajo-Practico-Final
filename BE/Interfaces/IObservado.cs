using System;

namespace BE.Interfaces
{
    public interface IObservado
    {
        void Suscribir(IObserver observador);
        void Desuscribir(IObserver observador);
        void Notificar(EventoBitacora alerta);
    }
}