using System;
using System.IO;
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
        private readonly string rutaArchivoConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "idioma_config.json");
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
            PersistirIdioma(idioma);
            Notificar();
        }

        private void PersistirIdioma(Idioma idioma)
        {
            try
            {
                if (idioma != null)
                {
                    string json = $"{{\r\n  \"IdiomaId\": {idioma.Id}\r\n}}";
                    File.WriteAllText(rutaArchivoConfig, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Error al persistir idioma: " + ex.Message);
                GestorEventos.Instancia.Notificar(new EventoBitacora("SISTEMA", "WARNING", "Error al persistir idioma: " + ex.Message));
            }
        }

        public int ObtenerIdiomaPersistido()
        {
            try
            {
                if (File.Exists(rutaArchivoConfig))
                {
                    string contenido = File.ReadAllText(rutaArchivoConfig);
                    int index = contenido.IndexOf("\"IdiomaId\"");
                    if (index != -1)
                    {
                        int colonIndex = contenido.IndexOf(":", index);
                        if (colonIndex != -1)
                        {
                            string valPart = "";
                            for (int i = colonIndex + 1; i < contenido.Length; i++)
                            {
                                char c = contenido[i];
                                if (char.IsDigit(c))
                                {
                                    valPart += c;
                                }
                                else if (valPart.Length > 0)
                                {
                                    break;
                                }
                            }
                            if (int.TryParse(valPart, out int id))
                            {
                                return id;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Error al leer idioma persistido: " + ex.Message);
                GestorEventos.Instancia.Notificar(new EventoBitacora("SISTEMA", "WARNING", "Error al leer idioma persistido: " + ex.Message));
            }
            return 0;
        }

        public void NotificarCambio()
        {
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
