using System;
using System.IO;

namespace Servicios
{
    public static class ArchivoAtomico
    {
        public static void Guardar(string destino, Action<Stream> escribir)
        {
            if (string.IsNullOrWhiteSpace(destino)) throw new ArgumentException("Debe indicar el archivo de destino.");
            string ruta = Path.GetFullPath(destino);
            string carpeta = Path.GetDirectoryName(ruta);
            if (!Directory.Exists(carpeta) || string.IsNullOrEmpty(Path.GetFileName(ruta)))
                throw new DirectoryNotFoundException("La carpeta de destino no existe.");
            string temporal = Path.Combine(carpeta, ".perfumeria-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using (var stream = new FileStream(temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    escribir(stream);
                if (File.Exists(ruta)) File.Replace(temporal, ruta, null);
                else File.Move(temporal, ruta);
            }
            finally
            {
                if (File.Exists(temporal)) File.Delete(temporal);
            }
        }
    }
}
