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
using UI;
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
            using (var host = new Form { Width = 1180, Height = 740 })
            using (var ui = new UC_Aprovisionamiento(actor) { ModoPrueba = true })
            {
                connection.Open();
                host.Controls.Add(ui); host.Show(); Application.DoEvents();
                ui.Controls.OfType<TabControl>().Single().SelectedIndex = 1;
                Assert(ui.UltimoError == null, "La grilla carga sin errores");
                Assert(!ui.Controls.Find("txtProveedorRazon", true).Any() && !ui.Controls.Find("txtProductoCodigo", true).Any(),
                    "Compras presenta cotizaciones y ordenes sin formularios de alta incrustados");
                var productosCombo = Find<ComboBox>(ui, "cmbOrdenProducto");
                var proveedoresCombo = Find<ComboBox>(ui, "cmbOrdenProveedor");
                int productoAnterior = (int)productosCombo.SelectedValue;
                int proveedorAnterior = (int)proveedoresCombo.SelectedValue;
                int productosEnLista = productosCombo.Items.Count, proveedoresEnLista = proveedoresCombo.Items.Count;
                int proveedoresAntes = Count("Proveedor"), productosAntes = Count("Producto");
                using (var alta = new FormAltaProveedor())
                {
                    InvokeButton(alta, "btnGuardarCatalogo");
                    Assert(Find<Label>(alta, "lblErrorCatalogo").Text.Contains("razón social") && Count("Proveedor") == proveedoresAntes,
                        "Alta separada mantiene validacion BLL y no cierra ni guarda datos invalidos");
                    Find<TextBox>(alta, "txtProveedorRazon").Text = "ZZ Proveedor de prueba de refresco";
                    Find<TextBox>(alta, "txtProveedorCuit").Text = "98123456789";
                    InvokeButton(alta, "btnGuardarCatalogo");
                    Assert(alta.DialogResult == DialogResult.OK && Count("Proveedor") == proveedoresAntes + 1, "Alta separada guarda proveedor real");
                }
                using (var cancelar = new FormAltaPerfume())
                {
                    Find<TextBox>(cancelar, "txtProductoCodigo").Text = "CANCELADO";
                    InvokeButton(cancelar, "btnCancelarCatalogo");
                    Assert(Count("Producto") == productosAntes, "Cancelar alta no modifica catalogos");
                }
                using (var alta = new FormAltaPerfume())
                {
                    Find<TextBox>(alta, "txtProductoCodigo").Text = "TEST-REFRESCO";
                    Find<TextBox>(alta, "txtProductoNombre").Text = "ZZ Perfume de prueba de refresco";
                    Find<NumericUpDown>(alta, "nudProductoPrecio").Value = 12300;
                    InvokeButton(alta, "btnGuardarCatalogo");
                    Assert(alta.DialogResult == DialogResult.OK && Count("Producto") == productosAntes + 1, "Alta separada guarda perfume real");
                }
                Assert(productosCombo.Items.Count == productosEnLista && proveedoresCombo.Items.Count == proveedoresEnLista,
                    "Fixture reproduce listas de una vista abierta antes de cambios externos");
                InvokeButton(ui, "btnActualizarOrdenes");
                Assert(ui.UltimoError == null && productosCombo.Items.Count == productosEnLista + 1 && proveedoresCombo.Items.Count == proveedoresEnLista + 1 &&
                    (int)productosCombo.SelectedValue == productoAnterior && (int)proveedoresCombo.SelectedValue == proveedorAnterior,
                    "Actualizar ordenes consulta catalogos nuevos y conserva las selecciones vigentes");
                int nuevoProducto = new ProductoBLL().Listar().Single(p => p.Codigo == "TEST-REFRESCO").IdProducto;
                var inventarioCombo = Find<ComboBox>(ui, "cmbInventarioProducto"); inventarioCombo.SelectedValue = nuevoProducto;
                new InventarioBLL().RegistrarIngresoLote(new Lote { IdProducto = nuevoProducto, IdProveedor = proveedorAnterior,
                    CodigoLote = "TEST-REFRESCO", Origen = "Argentina (prueba)", CantidadIngresada = 4 });
                InvokeButton(ui, "btnActualizarRecepcion");
                Assert(ui.UltimoError == null && Find<Label>(ui, "lblStock").Text.EndsWith("4") && Find<DataGridView>(ui, "dgvTrazabilidad").Rows.Count == 1,
                    "Actualizar recepcion vuelve a consultar stock y trazabilidad ingresados por otra sesion");
                var grillaOrdenes = Find<DataGridView>(ui, "dgvOrdenes");
                int ordenSeleccionada = (int)grillaOrdenes.CurrentRow.Cells["IdOrdenCompra"].Value;
                var nuevaOrden = Borrador(proveedorAnterior, productoAnterior);
                AprovisionamientoDAL.Instancia.InsertarOrdenes(new List<OrdenCompra> { nuevaOrden });
                InvokeButton(ui, "btnActualizarOrdenes");
                Assert(grillaOrdenes.Rows.Count == Count("OrdenCompra") && (int)grillaOrdenes.CurrentRow.Cells["IdOrdenCompra"].Value == ordenSeleccionada,
                    "Actualizar incorpora orden externa sin perder la fila seleccionada");
                InvokeButton(ui, "btnActualizarCotizaciones");
                Assert(ui.UltimoError == null && Find<ComboBox>(ui, "cmbCotizacionProducto").Items.Cast<Producto>().Any(p => p.IdProducto == nuevoProducto),
                    "Actualizar cotizaciones incorpora los catalogos actuales");
                int antesDeCrear = Count("OrdenCompra");
                InvokeButton(ui, "btnNuevoBorrador"); InvokeButton(ui, "btnCancelarNuevaOrden");
                Assert(Count("OrdenCompra") == antesDeCrear, "Cancelar el formulario desplegable no crea una compra");
                InvokeButton(ui, "btnNuevoBorrador"); Find<NumericUpDown>(ui, "nudOrdenPrecio").Value = 1000;
                InvokeButton(ui, "btnCrearOrden");
                Assert(ui.UltimoError == null && Count("OrdenCompra") == antesDeCrear + 1 &&
                    (int)grillaOrdenes.CurrentRow.Cells["IdOrdenCompra"].Value == Convert.ToInt32(Query("SELECT MAX(IdOrdenCompra) FROM OrdenCompra").Rows[0][0]),
                    "Nuevo borrador guarda la orden y selecciona la fila recien creada");
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
                {
                    Assert(soloContable.Controls.OfType<TabControl>().Single().TabPages.Count == 1, "Contabilidad no ve botones de importacion ni exportacion de Compras");
                    Assert(!soloContable.Controls.OfType<MenuStrip>().Any(), "El menu de altas respeta el actor de Compras");
                    var pendientes = Find<DataGridView>(soloContable, "dgvPendientes"); int cantidadPendiente = pendientes.Rows.Count;
                    int ordenPendiente = (int)pendientes.Rows[0].Cells["IdOrdenCompra"].Value;
                    new AprovisionamientoBLL().AprobarOrdenCompra(ordenPendiente, contable.Id);
                    InvokeButton(soloContable, "btnActualizarPendientes");
                    Assert(soloContable.UltimoError == null && pendientes.Rows.Count == cantidadPendiente - 1,
                        "Actualizar Contabilidad retira una orden aprobada fuera de la vista");
                }
                try { using (var prohibida = new FormAltaProveedor()) { } throw new Exception("Contabilidad pudo abrir un alta"); }
                catch (UnauthorizedAccessException) { Assert(true, "Las ventanas de alta verifican el permiso aunque se fuerce su apertura"); }
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
