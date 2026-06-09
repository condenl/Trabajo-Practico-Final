using BE;
using BE.Interfaces;
using System.Collections.Generic;

namespace Servicios
{
    public class GestorEventos : IObservado
    {
        private static GestorEventos instancia;
        private static readonly object bloqueo = new object();
        private List<IObserver> observadores = new List<IObserver>();

        private GestorEventos() { }

        public static GestorEventos Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new GestorEventos();
                    return instancia;
                }
            }
        }

        public void Suscribir(IObserver observador)
        {
            if (!observadores.Contains(observador))
                observadores.Add(observador);
        }

        public void Desuscribir(IObserver observador)
        {
            if (observadores.Contains(observador))
                observadores.Remove(observador);
        }

        public void Notificar(EventoBitacora evento)
        {
            foreach (var obs in observadores)
            {
                obs.Actualizar(evento);
            }
        }
    }
}