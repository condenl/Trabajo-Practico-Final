using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BE;
using BLL;
using DAL;
using UI.UserControls;

internal static class TestEvaluacionProveedores
{
    private static SqlConnection connection;
    private static int checks;
    private static void Assert(bool ok, string text) { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); }
    private static DataTable Query(string sql)
    { using (var cmd = new SqlCommand(sql, connection)) using (var adapter = new SqlDataAdapter(cmd)) { var result = new DataTable(); adapter.Fill(result); return result; } }
    private static int Number(string sql) { return Convert.ToInt32(Query(sql).Rows[0][0]); }
    private static void Exec(string sql) { using (var cmd = new SqlCommand(sql, connection)) cmd.ExecuteNonQuery(); }
    private static T Find<T>(Control c, string name) where T : Control { return (T)c.Controls.Find(name, true).Single(); }
    private static void Click(Control c, string name) { typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Find<Button>(c, name), new object[] { EventArgs.Empty }); }
    private static void ClickBar(Chart chart, int indice)
    {
        Application.DoEvents(); var area = chart.ChartAreas[0];
        int x = (int)area.AxisY.ValueToPixelPosition(chart.Series[0].Points[indice].YValues[0] / 2);
        int y = (int)area.AxisX.ValueToPixelPosition(indice + 1);
        typeof(Control).GetMethod("OnMouseClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(chart,
            new object[] { new MouseEventArgs(MouseButtons.Left, 1, x, y, 0) });
    }
    [STAThread]
    private static int Main(string[] args)
    {
        var users = new UsuarioBLL();
        try
        {
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString);
            if (!builder.InitialCatalog.StartsWith("Perfumeria_Evaluacion_Test_")) throw new Exception("Se requiere una base aislada de pruebas.");
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            var actor = users.Login("demo.dueno@perfumeria.example", "DemoDueno2026!");
            Assert(actor != null, "Login real de Compras/Dueno");
            if (args.Length == 1 && args[0] == "--app")
            {
                using (var f = new UI.FormPrincipal(actor)) Application.Run(f);
                return 0;
            }
            if (args.Length == 0)
            {
                using (var f = new UI.FormPrincipal(actor) { Text = "Prueba UX - Perfumeria" }) Application.Run(f);
                return 0;
            }
            using (connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                int producto = Number("SELECT IdProducto FROM Producto WHERE Codigo='DEMO-FLORAL'");
                int proveedor = Number("SELECT IdProveedor FROM Proveedor WHERE Cuit='99000000001'");
                // Demonstrate that the cheapest offer is not necessarily the recommended one.
                Exec(@"UPDATE Proveedor SET IndicePrestigio=20,AntiguedadMercado=1 WHERE Cuit='99000000002';
 UPDATE d SET PrecioUnitario=8000 FROM DetalleCotizacionProveedor d JOIN CotizacionProveedor c ON c.IdCotizacion=d.IdCotizacion
 JOIN Proveedor p ON p.IdProveedor=c.IdProveedor JOIN Producto pr ON pr.IdProducto=d.IdProducto WHERE p.Cuit='99000000002' AND pr.Codigo='DEMO-FLORAL';");
                int ordenes = Number("SELECT COUNT(*) FROM OrdenCompra"), movimientos = Number("SELECT COUNT(*) FROM MovimientoStock");
                var bll = new EvaluacionProveedorBLL();
                using (var form = new Form { Width = 1180, Height = 820 })
                using (var ui = new UC_EvaluacionProveedores { ModoPrueba = true })
                {
                    form.Controls.Add(ui); form.Show(); Application.DoEvents();
                    Find<ComboBox>(ui, "cmbEvaluacionProducto").SelectedValue = producto;
                    Click(ui, "btnCompararProveedores");
                    Application.DoEvents();
                    var grid = Find<DataGridView>(ui, "dgvEvaluacion");
                    Assert(grid.Rows.Count > 0, "La grilla vinculada presenta los candidatos");
                    Assert(grid.CurrentRow != null && Find<Button>(ui, "btnConfirmarProveedor").Enabled,
                        "La recomendacion queda seleccionada desde el resumen visual sin abrir el listado");
                    grid.CurrentCell = grid.Rows[0].Cells[0];
                    Assert(ui.UltimoError == null, "Boton Comparar usa consultas SQL y guarda analisis");
                    var resumen = (ResumenEvaluacion)Find<ComboBox>(ui, "cmbHistorialEvaluacion").Items[0];
                    var e = bll.Obtener(resumen.IdEvaluacion);
                    Assert(e.IdProveedorRecomendado == proveedor, "Ranking cruza costo, prestigio y antiguedad");
                    Assert(e.Resultados.Any(r => r.PrecioUnitario < e.Resultados[0].PrecioUnitario && r.Elegible), "Existe una oferta mas barata que la recomendada");
                    Assert(e.Datos.StockDisponible == 14 && e.Datos.ComprasPendientes == 16, "Planificacion usa stock y pedidos pendientes reales");
                    Assert(Number("SELECT COUNT(*) FROM OrdenCompra") == ordenes && Number("SELECT COUNT(*) FROM MovimientoStock") == movimientos, "Comparar no compra ni modifica stock");
                    var graficoPuntaje = Find<Chart>(ui, "chartPuntajes");
                    var graficoMargen = Find<Chart>(ui, "chartMargenes");
                    Assert(graficoPuntaje.Bottom <= graficoPuntaje.Parent.ClientSize.Height && graficoMargen.Bottom <= graficoMargen.Parent.ClientSize.Height,
                        "Ambos graficos caben en su area y no quedan recortados por la fila del formulario");
                    var tamanoInicial = form.ClientSize;
                    form.ClientSize = new System.Drawing.Size(1000, 570); Application.DoEvents();
                    Assert(Math.Abs(graficoPuntaje.ChartAreas[0].AxisX.ValueToPixelPosition(1) - graficoPuntaje.ChartAreas[0].AxisX.ValueToPixelPosition(2)) >= 14,
                        "La ventana reducida conserva espacio legible entre proveedores del grafico");
                    form.ClientSize = tamanoInicial; Application.DoEvents();
                    Assert(graficoPuntaje.Series[0].Points.Count == e.Resultados.Count &&
                        graficoPuntaje.Series[0].Points.Zip(e.Resultados, (p,r) => (int)p.Tag == r.IdProveedor && Math.Abs(p.YValues[0] - (double)r.PuntajePonderadoFinal) < 0.00001).All(v => v),
                        "Las barras de puntaje corresponden a proveedores y valores reales del ranking");
                    Assert(graficoMargen.Series[0].Points.Zip(e.Resultados, (p,r) => Math.Abs(p.YValues[0] - (double)r.MargenEstimado) < 0.00001).All(v => v),
                        "El grafico de margen muestra porcentajes reales, sin mezclar importes");
                    ClickBar(graficoMargen, 1);
                    string alternativaElegida = e.Resultados[1].Proveedor;
                    Assert(Find<Label>(ui, "lblDetalleProveedor").Text.StartsWith(e.Resultados[1].Proveedor),
                        "Elegir una barra cambia el proveedor de la decision aunque el listado este oculto");
                    var evaluacionOriginal = e;
                    decimal precioOriginal = e.Resultados.Single(r => r.IdProveedor == proveedor).PrecioUnitario;
                    Exec("UPDATE DetalleCotizacionProveedor SET PrecioUnitario=PrecioUnitario+100 WHERE IdProducto=" + producto +
                        " AND IdCotizacion=" + e.Resultados.Single(r => r.IdProveedor == proveedor).IdCotizacion.Value);
                    Find<TextBox>(ui, "txtMotivoSeleccion").Text = "Fundamento en preparacion que debe conservarse al actualizar.";
                    Click(ui, "btnActualizarEvaluacion");
                    e = bll.Obtener(((ResumenEvaluacion)Find<ComboBox>(ui, "cmbHistorialEvaluacion").Items[0]).IdEvaluacion);
                    Assert(ui.UltimoError == null && e.IdEvaluacion != evaluacionOriginal.IdEvaluacion && e.Resultados.Single(r => r.IdProveedor == proveedor).PrecioUnitario == precioOriginal + 100,
                        "Actualizar vuelve a consultar y comparar despues de una cotizacion modificada por otra sesion");
                    Assert(bll.Obtener(evaluacionOriginal.IdEvaluacion).Resultados.Single(r => r.IdProveedor == proveedor).PrecioUnitario == precioOriginal,
                        "Actualizar no sobrescribe la instantanea historica anterior");
                    Assert(Find<TextBox>(ui, "txtMotivoSeleccion").Text.Contains("conservarse") && (int)Find<ComboBox>(ui, "cmbEvaluacionProducto").SelectedValue == producto &&
                        Find<Label>(ui, "lblDetalleProveedor").Text.StartsWith(alternativaElegida),
                        "Actualizar conserva el producto, proveedor y fundamento en preparacion");
                    ClickBar(graficoPuntaje, 0);
                    Find<TextBox>(ui, "txtMotivoSeleccion").Clear();
                    Click(ui, "btnConfirmarProveedor"); Assert(ui.UltimoError != null, "Confirmar exige fundamento");
                    Find<TextBox>(ui, "txtMotivoSeleccion").Text = "Se elige por su equilibrio entre precio, prestigio y trayectoria.";
                    Click(ui, "btnConfirmarProveedor"); Assert(ui.UltimoError == null && bll.Obtener(e.IdEvaluacion).IdProveedorSeleccionado == proveedor, "Decision justificada queda persistida");
                    Click(ui, "btnOrdenEvaluacion"); Assert(ui.UltimoError == null, "Boton crea borrador desde la decision");
                    int orden = bll.Obtener(e.IdEvaluacion).IdOrdenCompra.Value;
                    var o = AprovisionamientoDAL.Instancia.ObtenerOrden(orden);
                    Assert(o.Estado == EstadoOrdenCompra.Borrador && o.IdUsuarioAprobadorContable == null && o.Detalles[0].CantidadRecibida == 0, "No saltea Contabilidad ni genera recepcion");
                    Assert(o.Detalles[0].IdProducto == producto && o.Detalles[0].Cantidad == e.Solicitud.Cantidad && o.Detalles[0].PrecioUnitario == e.Resultados[0].PrecioUnitario, "Borrador conserva producto, cantidad y precio evaluados");
                    Assert(bll.CrearOrdenBorrador(e.IdEvaluacion) == orden && Number("SELECT COUNT(*) FROM OrdenCompra") == ordenes + 1, "Reintentar no duplica orden");
                    Assert(Number("SELECT COUNT(*) FROM MovimientoStock") == movimientos, "Seleccion y borrador conservan stock");
                    int evaluacionesGuardadas = Number("SELECT COUNT(*) FROM EvaluacionProveedor");
                    Click(ui, "btnActualizarEvaluacion");
                    Assert(ui.UltimoError == null && Number("SELECT COUNT(*) FROM EvaluacionProveedor") == evaluacionesGuardadas &&
                        bll.Obtener(e.IdEvaluacion).IdOrdenCompra == orden && !Find<Button>(ui, "btnOrdenEvaluacion").Enabled,
                        "Actualizar una decision confirmada conserva su historial y el unico borrador vinculado");
                }
                var s = new SolicitudEvaluacion { IdProducto = producto, Cantidad = 3 };
                var antes = bll.Evaluar(s);
                Exec("UPDATE Proveedor SET IndicePrestigio=IndicePrestigio-1 WHERE IdProveedor=" + proveedor);
                try { bll.SeleccionarPrioritario(antes.IdEvaluacion, proveedor, "Intento con datos anteriores a la modificacion."); throw new Exception("Se acepto una evaluacion obsoleta"); }
                catch (InvalidOperationException) { }
                Assert(!bll.Obtener(antes.IdEvaluacion).IdProveedorSeleccionado.HasValue, "Datos cambiados impiden confirmar sin escrituras parciales");
                var nueva = bll.Evaluar(s); bll.SeleccionarPrioritario(nueva.IdEvaluacion, nueva.IdProveedorRecomendado.Value, "Decision actual para verificar concurrencia.");
                var concurrentes = new[] { Task.Factory.StartNew(() => bll.CrearOrdenBorrador(nueva.IdEvaluacion)), Task.Factory.StartNew(() => bll.CrearOrdenBorrador(nueva.IdEvaluacion)) };
                Task.WaitAll(concurrentes);
                Assert(concurrentes[0].Result == concurrentes[1].Result && Number("SELECT COUNT(*) FROM OrdenCompra") == ordenes + 2, "Dos peticiones concurrentes crean exactamente un borrador");
                var stale = bll.Evaluar(s); bll.SeleccionarPrioritario(stale.IdEvaluacion, stale.IdProveedorRecomendado.Value, "Seleccion previa para verificar cambio de stock.");
                int lote = Number("SELECT TOP(1) IdLote FROM Lote WHERE IdProducto=" + producto + " AND CantidadDisponible>0");
                Exec("UPDATE Lote SET CantidadDisponible=CantidadDisponible-1 WHERE IdLote=" + lote);
                try { bll.CrearOrdenBorrador(stale.IdEvaluacion); throw new Exception("Se acepto stock obsoleto"); }
                catch (InvalidOperationException) { }
                finally { Exec("UPDATE Lote SET CantidadDisponible=CantidadDisponible+1 WHERE IdLote=" + lote); }
                Assert(!bll.Obtener(stale.IdEvaluacion).IdOrdenCompra.HasValue && Number("SELECT COUNT(*) FROM OrdenCompra") == ordenes + 2, "Revalida stock antes del borrador y no deja orden parcial");
                int citrico = Number("SELECT IdProducto FROM Producto WHERE Codigo='DEMO-CITRICO'");
                Assert(!bll.Evaluar(new SolicitudEvaluacion { IdProducto = citrico }).IdProveedorRecomendado.HasValue, "Un solo proveedor cotizado no genera recomendacion");
                Assert(bll.ConsultarHistorial(producto).Count >= 4, "Historial recuperable con evidencia y decisiones");
                users.Logout(); users.Login("demo.contabilidad@perfumeria.example", "DemoContabilidad2026!");
                try { bll.Evaluar(s); throw new Exception("Contabilidad no debe seleccionar proveedores"); } catch (UnauthorizedAccessException) { }
                Assert(true, "Evaluacion respeta el actor de Compras");
                Console.WriteLine("RESULTADO: " + checks + " verificaciones OK; BASE=" + builder.InitialCatalog);
            }
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
        finally { users.Logout(); }
    }
}
