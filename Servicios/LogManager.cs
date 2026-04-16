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

        public void RegistrarAlerta(Alerta alerta)
        {
            try
            {
                string linea = $"{alerta.FechaHora:yyyy-MM-dd HH:mm:ss} | {alerta.Nivel} | {alerta.UsuarioGenerador} | {alerta.Mensaje}";
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