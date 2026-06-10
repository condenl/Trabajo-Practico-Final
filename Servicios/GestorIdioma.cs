using System.Collections.Generic;
using BE;
using BE.Interfaces;

namespace Servicios
{
    public class GestorIdioma
    {
        private static GestorIdioma instancia;
        private static readonly object bloqueo = new object();
        private List<IIdiomaObserver> observadores = new List<IIdiomaObserver>();
        public Idioma IdiomaActual { get; private set; }

        private GestorIdioma() { }

        public static GestorIdioma Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new GestorIdioma();
                    return instancia;
                }
            }
        }

        public void Suscribir(IIdiomaObserver observador)
        {
            if (!observadores.Contains(observador))
                observadores.Add(observador);
        }

        public void Desuscribir(IIdiomaObserver observador)
        {
            if (observadores.Contains(observador))
                observadores.Remove(observador);
        }

        public void SeleccionarIdioma(Idioma idioma)
        {
            IdiomaActual = idioma;
            Notificar();
        }

        private void Notificar()
        {
            foreach (var obs in observadores)
            {
                obs.ActualizarIdioma(IdiomaActual);
            }
        }
    }
}
