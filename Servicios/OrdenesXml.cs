using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BE;

namespace Servicios
{
    public static class OrdenesXml
    {
        public const int MaximoOrdenes = 1000;
        public const long MaximoBytes = 10 * 1024 * 1024;

        public static void Validar(ArchivoOrdenesCompra archivo)
        {
            if (archivo == null || archivo.Version != 1)
                throw new InvalidDataException("El archivo debe usar el formato ArchivoOrdenesCompra, versión 1.");
            if (archivo.Ordenes == null || archivo.Ordenes.Count == 0 || archivo.Ordenes.Count > MaximoOrdenes)
                throw new InvalidDataException("El archivo debe contener entre 1 y " + MaximoOrdenes + " órdenes.");
            if (archivo.Ordenes.Any(o => o == null || o.IdOrdenOrigen <= 0) ||
                archivo.Ordenes.Select(o => o.IdOrdenOrigen).Distinct().Count() != archivo.Ordenes.Count)
                throw new InvalidDataException("Las órdenes deben tener identificadores de origen positivos y no repetidos.");
            int detalles = 0;
            foreach (OrdenCompraArchivo orden in archivo.Ordenes)
            {
                if (string.IsNullOrWhiteSpace(orden.ProveedorCuit) || orden.ProveedorCuit.Length != 11 || !orden.ProveedorCuit.All(char.IsDigit))
                    throw new InvalidDataException("CUIT inválido en la orden de origen " + orden.IdOrdenOrigen + ".");
                if (!Enum.IsDefined(typeof(EstadoOrdenCompra), orden.Estado) || orden.FechaEmision == DateTime.MinValue ||
                    orden.Detalles == null || orden.Detalles.Count == 0 || orden.Detalles.Count > 500)
                    throw new InvalidDataException("Estado, fecha o detalles inválidos en la orden de origen " + orden.IdOrdenOrigen + ".");
                if ((orden.ProveedorNombre ?? "").Length > 150 || (orden.CreadaPor ?? "").Length > 200 || (orden.MotivoRechazo ?? "").Length > 500)
                    throw new InvalidDataException("Un texto de la orden excede la longitud admitida.");
                detalles += orden.Detalles.Count;
                if (detalles > 10000) throw new InvalidDataException("El archivo supera los 10000 detalles permitidos.");
                foreach (DetalleOrdenArchivo detalle in orden.Detalles)
                {
                    if (detalle == null || string.IsNullOrWhiteSpace(detalle.ProductoCodigo) || detalle.ProductoCodigo.Length > 50 ||
                        (detalle.ProductoNombre ?? "").Length > 150 || detalle.Cantidad <= 0 ||
                        detalle.CantidadRecibida < 0 || detalle.CantidadRecibida > detalle.Cantidad ||
                        detalle.PrecioUnitario <= 0 || detalle.PrecioUnitario > 9999999999999999.99m ||
                        decimal.Round(detalle.PrecioUnitario, 2) != detalle.PrecioUnitario)
                        throw new InvalidDataException("Detalle inválido en la orden de origen " + orden.IdOrdenOrigen + ".");
                }
                if (orden.Detalles.Select(d => d.ProductoCodigo.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != orden.Detalles.Count)
                    throw new InvalidDataException("Hay productos repetidos en la orden de origen " + orden.IdOrdenOrigen + ".");
                decimal total;
                try { total = orden.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario); }
                catch (OverflowException) { throw new InvalidDataException("El importe de la orden excede el rango permitido."); }
                if (total > 9999999999999999.99m || total != orden.Total)
                    throw new InvalidDataException("El total no coincide con sus detalles o excede el rango permitido en la orden de origen " + orden.IdOrdenOrigen + ".");
            }
        }

        public static void Guardar(string ruta, ArchivoOrdenesCompra archivo)
        {
            Validar(archivo);
            ArchivoAtomico.Guardar(ruta, stream =>
            {
                var serializer = new XmlSerializer(typeof(ArchivoOrdenesCompra));
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");
                using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true, CloseOutput = false }))
                    serializer.Serialize(writer, archivo, namespaces);
                if (stream.Length > MaximoBytes) throw new InvalidDataException("El XML supera el límite de 10 MB.");
            });
        }

        public static ArchivoOrdenesCompra Leer(string ruta)
        {
            using (var stream = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > MaximoBytes) throw new InvalidDataException("El XML supera el límite de 10 MB.");
                try
                {
                    var serializer = new XmlSerializer(typeof(ArchivoOrdenesCompra));
                    serializer.UnknownNode += (sender, args) => { throw new InvalidDataException("El XML contiene un nodo no reconocido: " + args.Name); };
                    serializer.UnknownAttribute += (sender, args) => { throw new InvalidDataException("El XML contiene un atributo no reconocido: " + args.Attr.Name); };
                    var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null,
                        MaxCharactersInDocument = MaximoBytes, IgnoreWhitespace = true, CloseInput = false };
                    using (var reader = XmlReader.Create(stream, settings))
                    {
                        var archivo = (ArchivoOrdenesCompra)serializer.Deserialize(reader);
                        // Read to EOF as well: a second root or trailing malformed markup must not be ignored.
                        while (reader.Read()) { }
                        Validar(archivo);
                        return archivo;
                    }
                }
                catch (InvalidOperationException error) { throw new InvalidDataException("El XML no tiene el formato de órdenes esperado.", error); }
                catch (XmlException error) { throw new InvalidDataException("El XML está mal formado o contiene una DTD no permitida.", error); }
            }
        }
    }
}
