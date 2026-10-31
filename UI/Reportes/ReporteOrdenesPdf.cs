using System;
using System.Globalization;
using System.Text;
using BE;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Servicios;

namespace UI.Reportes
{
    public static class ReporteOrdenesPdf
    {
        public static void Guardar(string ruta, ArchivoOrdenesCompra archivo)
        {
            OrdenesXml.Validar(archivo);
            var cultura = CultureInfo.GetCultureInfo("es-AR");
            var documento = new Document();
            documento.Info.Title = "Detalle de órdenes de compra";
            documento.Info.Author = "Perfumería";
            var normal = documento.Styles[StyleNames.Normal] ?? throw new InvalidOperationException("No se encontró el estilo normal del reporte.");
            normal.Font.Name = "Arial";
            normal.Font.Size = 10;
            normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(5);
            foreach (OrdenCompraArchivo orden in archivo.Ordenes)
            {
                var seccion = documento.AddSection();
                seccion.PageSetup.PageFormat = PageFormat.A4;
                seccion.PageSetup.LeftMargin = seccion.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
                seccion.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
                seccion.PageSetup.BottomMargin = Unit.FromCentimeter(2);
                var titulo = seccion.AddParagraph("Perfumería - Órdenes de compra");
                titulo.Format.Font.Size = 14;
                titulo.Format.Font.Bold = true;
                seccion.AddParagraph("Generado: " + archivo.FechaExportacionUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm", cultura) + " | Órdenes del listado: " + archivo.Ordenes.Count);
                var encabezado = seccion.AddParagraph("Orden " + orden.IdOrdenOrigen + " - " + orden.Estado);
                encabezado.Format.Font.Bold = true;
                seccion.AddParagraph(AjustarTexto("Proveedor: " + orden.ProveedorNombre + " | CUIT: " + orden.ProveedorCuit, 40));
                seccion.AddParagraph(AjustarTexto("Fecha: " + orden.FechaEmision.ToString("dd/MM/yyyy HH:mm", cultura) + " | Creada por: " + orden.CreadaPor, 40));
                if (orden.FechaAprobacion.HasValue)
                    seccion.AddParagraph("Aprobación: " + orden.FechaAprobacion.Value.ToString("dd/MM/yyyy HH:mm", cultura) + " | Usuario: " + orden.IdUsuarioAprobadorContable);
                if (!string.IsNullOrWhiteSpace(orden.MotivoRechazo)) seccion.AddParagraph(AjustarTexto("Motivo de rechazo: " + orden.MotivoRechazo, 40));
                var tabla = seccion.AddTable();
                tabla.Borders.Width = Unit.FromPoint(0.5);
                tabla.Format.Font.Size = 9;
                tabla.TopPadding = tabla.BottomPadding = Unit.FromPoint(4);
                foreach (double ancho in new[] { 2.6, 5.4, 1.5, 1.5, 3.3, 3.7 }) tabla.AddColumn(Unit.FromCentimeter(ancho));
                var titulos = tabla.AddRow();
                titulos.HeadingFormat = true;
                titulos.Format.Font.Bold = true;
                string[] nombres = { "Código", "Producto", "Cantidad", "Recibida", "Precio unitario", "Subtotal" };
                for (int i = 0; i < nombres.Length; i++) titulos.Cells[i].AddParagraph(nombres[i]);
                foreach (DetalleOrdenArchivo detalle in orden.Detalles)
                {
                    var fila = tabla.AddRow();
                    string[] valores = { detalle.ProductoCodigo, detalle.ProductoNombre, detalle.Cantidad.ToString(cultura),
                        detalle.CantidadRecibida.ToString(cultura), "$ " + detalle.PrecioUnitario.ToString("N2", cultura),
                        "$ " + (detalle.Cantidad * detalle.PrecioUnitario).ToString("N2", cultura) };
                    for (int i = 0; i < valores.Length; i++)
                    {
                        int maximoSinEspacios = i == 0 ? 10 : i == 1 ? 16 : i < 4 ? 6 : 15;
                        fila.Cells[i].AddParagraph(AjustarTexto(valores[i], maximoSinEspacios));
                        if (i >= 2) fila.Cells[i].Format.Alignment = ParagraphAlignment.Right;
                    }
                }
                var total = seccion.AddParagraph("Total de la orden: $ " + orden.Total.ToString("N2", cultura));
                total.Format.Font.Bold = true;
                total.Format.Alignment = ParagraphAlignment.Right;
                total.Format.SpaceBefore = Unit.FromPoint(8);
                var pie = seccion.Footers.Primary.AddParagraph("Página ");
                pie.AddPageField();
                pie.Format.Alignment = ParagraphAlignment.Right;
            }
            var renderer = new PdfDocumentRenderer { Document = documento };
            renderer.RenderDocument();
            ArchivoAtomico.Guardar(ruta, stream => renderer.PdfDocument.Save(stream, false));
        }

        // MigraDoc no divide palabras largas: insertar saltos solo en la presentación,
        // sin truncar ni modificar los valores originales o el archivo XML.
        private static string AjustarTexto(string valor, int maximoSinEspacios)
        {
            var resultado = new StringBuilder();
            var elementos = StringInfo.GetTextElementEnumerator(valor ?? "");
            int consecutivos = 0;
            while (elementos.MoveNext())
            {
                string elemento = elementos.GetTextElement();
                if (char.IsWhiteSpace(elemento, 0)) consecutivos = 0;
                else
                {
                    if (consecutivos == maximoSinEspacios)
                    {
                        resultado.Append('\n');
                        consecutivos = 0;
                    }
                    consecutivos++;
                }
                resultado.Append(elemento);
            }
            return resultado.ToString();
        }
    }
}
