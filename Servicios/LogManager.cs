using System;
using System.IO;
using BE;
using BE.Interfaces;

namespace Servicios
{
    public class LogManager : IObserver
    {
        #region Singleton
        private static LogManager instancia;
        private static readonly object bloqueo = new object();

        private LogManager() { }

        public static LogManager Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new LogManager();
                    return instancia;
                }
            }
        }
        #endregion

        private string rutaArchivo = "bitacora_alertas.log";

        // IMPLEMENTAMOS EL MÉTODO DEL CONTRATO
        public void Actualizar(EventoBitacora evento)
        {
            // Magia pura: El LogManager recibe TODOS los eventos, pero él mismo
            // decide que SOLO le interesan los fallos y bloqueos de seguridad.
            // La base de datos guarda todo, el archivo de texto guarda solo lo crítico.
            if (evento.Actividad == "BLOQUEO" || evento.Actividad == "LOGIN_FALLIDO" || evento.Actividad == "SEGURIDAD")
            {
                RegistrarAlerta(evento);
            }
        }

        public void RegistrarAlerta(EventoBitacora evento)
        {
            try
            {
                string linea = $"{evento.FechaHora:yyyy-MM-dd HH:mm:ss} | {evento.Actividad} | {evento.UsuarioEmail} | {evento.InformacionAsociada}";

                using (FileStream fs = new FileStream(rutaArchivo, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(linea);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}