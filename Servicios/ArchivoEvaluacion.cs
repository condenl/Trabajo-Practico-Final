using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BE;

namespace Servicios
{
    public static class ArchivoEvaluacion
    {
        public static string Serializar<T>(T valor)
        {
            using (var writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture))
            { new XmlSerializer(typeof(T)).Serialize(writer, valor); return writer.ToString(); }
        }
        public static EvaluacionProveedor Leer(string xml)
        {
            using (var text = new StringReader(xml))
            using (var reader = XmlReader.Create(text, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 5000000 }))
                return (EvaluacionProveedor)new XmlSerializer(typeof(EvaluacionProveedor)).Deserialize(reader);
        }
        public static string Firma(DatosEvaluacion datos)
        {
            using (var sha = SHA256.Create())
                return System.BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(Serializar(datos)))).Replace("-", "");
        }
    }
}
