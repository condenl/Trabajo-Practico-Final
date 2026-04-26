using System;
using System.IO;
using System.Text;
using BE;

namespace Servicios
{
    public class LogManager
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
            catch (Exception)
            {
            }
        }
    }
}