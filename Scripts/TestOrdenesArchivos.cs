using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BE;
using BLL;
using DAL;
using Servicios;
using UI.UserControls;

internal static class TestOrdenesArchivos
{
    private static readonly UsuarioBLL Usuarios = new UsuarioBLL();
    private static int assertions;
    private static SqlConnection connection;
    private static void Assert(bool condition, string text)
    {
        if (!condition) throw new Exception(text);
        assertions++; Console.WriteLine("PASS " + text);
    }
    private static DataTable Query(string sql)
    {
        using (var command = new SqlCommand(sql, connection))
        using (var adapter = new SqlDataAdapter(command)) { var table = new DataTable(); adapter.Fill(table); return table; }
    }
    private static int Count(string table) { return Convert.ToInt32(Query("SELECT COUNT(*) FROM dbo.[" + table + "]").Rows[0][0]); }
    private static T Find<T>(Control root, string name) where T : Control { return (T)root.Controls.Find(name, true).Single(); }
    private static void InvokeButton(Control root, string name)
    {
        // Component event test, no desktop input automation or file-dialog bypass in normal app mode.
        typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(Find<Button>(root, name), new object[] { EventArgs.Empty });
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString);
            if (!builder.InitialCatalog.StartsWith("Perfumeria_Archivos_Test_", StringComparison.Ordinal))
                throw new Exception("Solo se admiten las bases aisladas del script test-ordenes-archivos.ps1.");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var actor = Usuarios.Login("demo.dueno@perfumeria.example", "DemoDueno2026!");
            if (actor == null) throw new Exception("No se pudo iniciar la sesion demo.");
            if (args.Length == 0)
            {
                // Optional visual test host: launch this local executable through Computer Use.
                using (var form = new Form { Text = "Prueba - Ordenes PDF y XML", Width = 1180, Height = 740 })
                {
                    var ui = new UC_Aprovisionamiento(actor);
                    form.Controls.Add(ui);
                    ui.Controls.OfType<TabControl>().Single().SelectedIndex = 1;
                    Application.Run(form);
                }
                return 0;
            }
            if (args.Length != 1 || args[0] != "--test") throw new Exception("Uso: TestOrdenesArchivos [--test]");
            using (connection = new SqlConnection(builder.ConnectionString))
            using (var ui = new UC_Aprovisionamiento(actor) { ModoPrueba = true })
            {
                connection.Open();
                ui.Controls.OfType<TabControl>().Single().SelectedIndex = 1;
                Assert(ui.UltimoError == null, "La grilla carga sin errores");
                int inicial = Count("OrdenCompra"), detalles = Count("DetalleOrdenCompra"), stock = Count("MovimientoStock");
                var ejemploDemo = OrdenesXml.Leer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ordenes-compra-3.xml"));
                Assert(ejemploDemo.Ordenes.Count == 3 && new IntercambioOrdenesBLL().ValidarImportacion(ejemploDemo) == 3,
                    "Archivo XML de ejemplo incluido contiene tres ordenes compatibles con los catalogos demo");
                InvokeButton(ui, "btnSampleOrdenesXml");
                string ejemploRuta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample-ordenes.xml");
                File.WriteAllText(ejemploRuta, Clipboard.GetText(), new System.Text.UTF8Encoding(false));
                var ejemplo = OrdenesXml.Leer(ejemploRuta);
                Assert(ui.UltimoError == null && ejemplo.Ordenes.Count == 3 &&
                    new IntercambioOrdenesBLL().ValidarImportacion(ejemplo) == 3, "Sample XML copia tres ordenes importables del catalogo actual al portapapeles");
                Assert(Count("OrdenCompra") == inicial && Count("DetalleOrdenCompra") == detalles &&
                    Count("MovimientoStock") == stock, "Generar ejemplo no crea compras ni movimientos");
                int unidades = Convert.ToInt32(Query("SELECT SUM(CantidadDisponible) FROM Lote").Rows[0][0]);
                string root = AppDomain.CurrentDomain.BaseDirectory;
                ui.RutaArchivoPrueba = Path.Combine(root, "ordenes.xml");
                InvokeButton(ui, "btnExportarOrdenesXml");
                Assert(ui.UltimoError == null && File.Exists(ui.RutaArchivoPrueba), "Boton Exportar XML genera archivo");
                var archivo = OrdenesXml.Leer(ui.RutaArchivoPrueba);
                Assert(archivo.Ordenes.Count == inicial && archivo.Ordenes.Sum(o => o.Detalles.Count) == detalles, "XML incluye listado completo y detalles, no solo fila seleccionada");
                Assert(archivo.Ordenes.Any(o => o.Estado == EstadoOrdenCompra.Recibida), "XML conserva estado historico");
                ui.RutaArchivoPrueba = Path.Combine(root, "ordenes.pdf");
                InvokeButton(ui, "btnReporteOrdenesPdf");
                Assert(ui.UltimoError == null && new FileInfo(ui.RutaArchivoPrueba).Length > 500, "Boton PDF genera reporte real");
                Assert(Count("OrdenCompra") == inicial && Count("MovimientoStock") == stock, "Exportaciones no modifican compras ni stock");

                ui.RutaArchivoPrueba = Path.Combine(root, "ordenes.xml");
                InvokeButton(ui, "btnImportarOrdenesXml");
                Assert(ui.UltimoError == null && ui.UltimoMensaje == null && Count("OrdenCompra") == inicial, "Cancelar confirmacion no importa ni anuncia exito");
                ui.ConfirmarImportacionPrueba = true;
                InvokeButton(ui, "btnImportarOrdenesXml");
                Assert(ui.UltimoError == null && Count("OrdenCompra") == inicial * 2, "Confirmar importa lote completo de nuevas ordenes");
                Assert(Find<DataGridView>(ui, "dgvOrdenes").Rows.Count == inicial * 2, "Grilla se actualiza tras importar");
                Assert(Count("DetalleOrdenCompra") == detalles * 2, "Todos los detalles importados se guardan");
                Assert(Convert.ToInt32(Query("SELECT COUNT(*) FROM OrdenCompra WHERE CreadaPor='demo.dueno@perfumeria.example' AND (Estado<>0 OR IdUsuarioAprobadorContable IS NOT NULL OR FechaAprobacion IS NOT NULL OR MotivoRechazo IS NOT NULL)").Rows[0][0]) == 0,
                    "Importacion no hereda aprobaciones, rechazo ni estado historico");
                Assert(Convert.ToInt32(Query("SELECT COUNT(*) FROM DetalleOrdenCompra d JOIN OrdenCompra o ON o.IdOrdenCompra=d.IdOrdenCompra WHERE o.CreadaPor='demo.dueno@perfumeria.example' AND CantidadRecibida<>0").Rows[0][0]) == 0, "Importacion no hereda recepciones");
                Assert(Count("MovimientoStock") == stock && Convert.ToInt32(Query("SELECT SUM(CantidadDisponible) FROM Lote").Rows[0][0]) == unidades, "Importacion conserva stock y movimientos");
                int despues = Count("OrdenCompra");
                archivo.Ordenes.Last().Detalles[0].ProductoCodigo = "NO-EXISTE";
                ui.RutaArchivoPrueba = Path.Combine(root, "catalogo-invalido.xml");
                OrdenesXml.Guardar(ui.RutaArchivoPrueba, archivo);
                InvokeButton(ui, "btnImportarOrdenesXml");
                Assert(ui.UltimoError != null && Count("OrdenCompra") == despues, "Catalogo invalido rechaza TODO el lote sin inserciones parciales");
                ui.RutaArchivoPrueba = Path.Combine(root, "malformado.xml");
                File.WriteAllText(ui.RutaArchivoPrueba, "<incompleto>");
                InvokeButton(ui, "btnImportarOrdenesXml");
                Assert(ui.UltimoError != null && Count("OrdenCompra") == despues, "Boton importar informa XML malformado sin alterar la base");

                int proveedor = Convert.ToInt32(Query("SELECT TOP 1 IdProveedor FROM Proveedor WHERE Activo=1").Rows[0][0]);
                int producto = Convert.ToInt32(Query("SELECT TOP 1 IdProducto FROM Producto WHERE Activo=1").Rows[0][0]);
                int antesDetalles = Count("DetalleOrdenCompra");
                try
                {
                    AprovisionamientoDAL.Instancia.InsertarOrdenes(new List<OrdenCompra> {
                        Borrador(proveedor, producto), Borrador(proveedor, int.MaxValue) });
                    throw new Exception("El lote DAL invalido debio fallar");
                }
                catch (SqlException) { }
                Assert(Count("OrdenCompra") == despues && Count("DetalleOrdenCompra") == antesDetalles, "SQL revierte incluso la primera orden si falla la segunda (rollback real)");

                ui.RutaArchivoPrueba = ejemploRuta;
                InvokeButton(ui, "btnImportarOrdenesXml");
                Assert(ui.UltimoError == null && Count("OrdenCompra") == despues + 3 &&
                    Find<DataGridView>(ui, "dgvOrdenes").Rows.Count == despues + 3,
                    "El XML copiado por Sample XML se importa como tres borradores y actualiza la grilla");
                Assert(Count("MovimientoStock") == stock, "Importar el ejemplo no altera stock");

                Usuarios.Logout();
                var contable = Usuarios.Login("demo.contabilidad@perfumeria.example", "DemoContabilidad2026!");
                Assert(contable != null, "Login de Contabilidad real");
                ui.RutaArchivoPrueba = Path.Combine(root, "denegado.xml");
                InvokeButton(ui, "btnExportarOrdenesXml");
                Assert(ui.UltimoError != null && !File.Exists(ui.RutaArchivoPrueba), "BLL rechaza exportacion aunque se fuerce un evento de otra sesion");
                using (var soloContable = new UC_Aprovisionamiento(contable) { ModoPrueba = true })
                    Assert(soloContable.Controls.OfType<TabControl>().Single().TabPages.Count == 1, "Contabilidad no ve botones de importacion ni exportacion de Compras");
                Console.WriteLine("RESULTADO: " + assertions + " verificaciones OK; BASE=" + builder.InitialCatalog);
                Console.WriteLine("ARTEFACTOS=" + root);
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { Usuarios.Logout(); }
    }
    private static OrdenCompra Borrador(int proveedor, int producto)
    {
        return new OrdenCompra { IdProveedor = proveedor, CreadaPor = "TEST-ROLLBACK", Total = 10m,
            Detalles = new List<DetalleOrdenCompra> { new DetalleOrdenCompra { IdProducto = producto, Cantidad = 1, PrecioUnitario = 10m } } };
    }
}
