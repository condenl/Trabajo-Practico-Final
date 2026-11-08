using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BE;
using BLL;

namespace UI.UserControls
{
    public class UC_EvaluacionProveedores : UserControl, IVistaConCambiosPendientes
    {
        public event Action<int> AbrirOrdenSolicitada;
        public bool TieneCambiosPendientes { get { return actual != null && !actual.IdProveedorSeleccionado.HasValue &&
            (!string.IsNullOrWhiteSpace(motivo.Text) || (proveedorSeleccionado.HasValue && proveedorSeleccionado != actual.IdProveedorRecomendado)); } }
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
        private readonly EvaluacionProveedorBLL bll = new EvaluacionProveedorBLL();
        private readonly ComboBox producto = new ComboBox { Name = "cmbEvaluacionProducto", Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown cantidad = Numero("nudEvaluacionCantidad", 1, 100000, 10);
        private readonly NumericUpDown precio = Numero("nudPesoPrecio", 0, 100, 50);
        private readonly NumericUpDown prestigio = Numero("nudPesoPrestigio", 0, 100, 30);
        private readonly NumericUpDown antiguedad = Numero("nudPesoAntiguedad", 0, 100, 20);
        private readonly DataGridView grilla = new DataGridView { Name = "dgvEvaluacion", Dock = DockStyle.Fill, ReadOnly = true,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoGenerateColumns = false, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private readonly Label contexto = new Label { Name = "lblResumenEvaluacion", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4), Text = "Seleccione un perfume y pulse Comparar." };
        private readonly Label detalle = new Label { Name = "lblDetalleProveedor", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4) };
        private readonly Label estado = new Label { Name = "lblActualizacionEvaluacion", AutoSize = true, Padding = new Padding(4) };
        private readonly TextBox motivo = new TextBox { Name = "txtMotivoSeleccion", Width = 310, MaxLength = 500 };
        private readonly ComboBox historial = new ComboBox { Name = "cmbHistorialEvaluacion", Width = 350, FormattingEnabled = true, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Chart puntajes = Grafico("chartPuntajes", "Puntaje de conveniencia / 100");
        private readonly Chart margenes = Grafico("chartMargenes", "Margen bruto estimado (%)");
        private readonly Label stock = Tarjeta("lblStockEvaluacion", "Stock utilizable");
        private readonly Label pendientes = Tarjeta("lblPendientesEvaluacion", "Por recibir");
        private readonly Label ventas = Tarjeta("lblVentasEvaluacion", "Ventas en 90 días");
        private readonly Label reposicion = Tarjeta("lblReposicionEvaluacion", "Reponer (30 días)");
        private Button confirmar, crear;
        private EvaluacionProveedor actual;
        private int? proveedorSeleccionado;
        private bool cargando;
        public bool ModoPrueba { get; set; }
        public string UltimoError { get; private set; }
        public string UltimoMensaje { get; private set; }

        public UC_EvaluacionProveedores()
        {
            Dock = DockStyle.Fill; BindingContext = new BindingContext();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 9, Padding = new Padding(8) };
            for (int i = 0; i < 9; i++) layout.RowStyles.Add(i == 4 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
            var filtros = Flujo(); Campo(filtros, "Perfume", producto); Campo(filtros, "Cantidad a comprar", cantidad);
            filtros.Controls.Add(Boton("Comparar", "btnCompararProveedores", Comparar));
            filtros.Controls.Add(Boton("Actualizar", "btnActualizarEvaluacion", Actualizar));
            var usarSugerida = Boton("Usar cantidad sugerida", "btnUsarCantidadSugerida", UsarCantidadSugerida);
            filtros.Controls.Add(usarSugerida);
            var criterios = Flujo(); criterios.Visible = false;
            var pie = Flujo(); pie.Visible = false;
            Campo(criterios, "Precio %", precio); Campo(criterios, "Prestigio %", prestigio); Campo(criterios, "Antigüedad %", antiguedad);
            criterios.Controls.Add(new Label { Text = "Los pesos deben sumar 100 %.", AutoSize = true, Margin = new Padding(6, 22, 6, 3) });
            filtros.Controls.Add(Boton("Criterios", "btnCriteriosEvaluacion", delegate { criterios.Visible = !criterios.Visible; }));
            filtros.Controls.Add(Boton("Historial", "btnHistorialEvaluacion", delegate { pie.Visible = !pie.Visible; }));
            layout.Controls.Add(filtros, 0, 0); layout.Controls.Add(criterios, 0, 1); layout.Controls.Add(contexto, 0, 2);
            var tarjetas = Flujo(); foreach (var tarjeta in new[] { stock, pendientes, ventas, reposicion }) tarjetas.Controls.Add(tarjeta);
            layout.Controls.Add(tarjetas, 0, 3);
            var resultados = new TabControl { Name = "tabsResultadosEvaluacion", Dock = DockStyle.Fill };
            var resumen = new TabPage("Resumen visual"); var listado = new TabPage("Listado completo");
            var graficos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            graficos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); graficos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            graficos.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            graficos.Controls.Add(puntajes, 0, 0); graficos.Controls.Add(margenes, 1, 0); resumen.Controls.Add(graficos);
            foreach (var col in new[] { new[] { "Proveedor", "Proveedor", "190" }, new[] { "PuntajePonderadoFinal", "Puntaje / 100", "95" },
                new[] { "PrecioUnitario", "Precio unitario ($)", "100" }, new[] { "CostoTotal", "Compra ($)", "105" },
                new[] { "MargenEstimado", "Margen bruto %", "95" }, new[] { "PlazoEntregaDias", "Plazo (días)", "80" }, new[] { "Elegible", "Elegible", "65" } })
                grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = col[0], HeaderText = col[1], FillWeight = int.Parse(col[2]),
                    DefaultCellStyle = new DataGridViewCellStyle { FormatProvider = Cultura, Format = col[0] == "PrecioUnitario" || col[0] == "CostoTotal" ? "N2" : col[0] == "MargenEstimado" || col[0] == "PuntajePonderadoFinal" ? "N1" : "" } });
            grilla.DefaultCellStyle.WrapMode = DataGridViewTriState.True; grilla.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grilla.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e) {
                if (e.ColumnIndex >= 0 && grilla.Columns[e.ColumnIndex].DataPropertyName == "Elegible" && e.Value is bool) { e.Value = (bool)e.Value ? "Sí" : "No"; e.FormattingApplied = true; }
            };
            grilla.SelectionChanged += delegate {
                var fila = grilla.CurrentRow == null ? null : grilla.CurrentRow.DataBoundItem as ResultadoProveedor;
                if (fila != null) proveedorSeleccionado = fila.IdProveedor;
                MostrarDetalle();
            };
            listado.Controls.Add(grilla); resultados.TabPages.Add(resumen); resultados.TabPages.Add(listado);
            layout.Controls.Add(resultados, 0, 4); layout.Controls.Add(detalle, 0, 5);
            var decision = Flujo(); Campo(decision, "Fundamento de la elección (10 a 500 caracteres)", motivo);
            confirmar = Boton("Confirmar proveedor", "btnConfirmarProveedor", Confirmar);
            crear = Boton("Crear orden en Borrador", "btnOrdenEvaluacion", CrearOrden);
            decision.Controls.Add(confirmar); decision.Controls.Add(crear); layout.Controls.Add(decision, 0, 6);
            Campo(pie, "Comparaciones guardadas", historial);
            historial.Format += delegate(object sender, ListControlConvertEventArgs e) { var r = (ResumenEvaluacion)e.ListItem; e.Value = "#" + r.IdEvaluacion + " · " + r.Fecha.ToString("dd/MM HH:mm") + " · " + r.ProveedorSeleccionado; };
            pie.Controls.Add(Boton("Abrir", "btnAbrirEvaluacion", delegate { var h = historial.SelectedItem as ResumenEvaluacion; if (h != null) Mostrar(bll.Obtener(h.IdEvaluacion)); }));
            layout.Controls.Add(pie, 0, 7); layout.Controls.Add(estado, 0, 8); Controls.Add(layout);
            producto.SelectedIndexChanged += delegate { if (!cargando) { Invalidar(); Ejecutar(CargarHistorial); } };
            foreach (var n in new[] { cantidad, precio, prestigio, antiguedad }) n.ValueChanged += delegate { if (!cargando) Invalidar(); };
            foreach (var chart in new[] { puntajes, margenes }) chart.MouseClick += ElegirDesdeGrafico;
            Resize += delegate { contexto.MaximumSize = detalle.MaximumSize = new Size(Math.Max(100, ClientSize.Width - 24), 0); };
            ActualizarBotones(); Ejecutar(ActualizarCatalogos);
        }

        private static NumericUpDown Numero(string nombre, int min, int max, int valor) { return new NumericUpDown { Name = nombre, Minimum = min, Maximum = max, Value = valor, Width = 80 }; }
        private static FlowLayoutPanel Flujo() { return new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(1) }; }
        private static void Campo(FlowLayoutPanel panel, string etiqueta, Control control)
        { var campo = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(3, 2, 6, 3) }; campo.Controls.Add(new Label { Text = etiqueta, AutoSize = true }); campo.Controls.Add(control); panel.Controls.Add(campo); }
        private Button Boton(string texto, string nombre, Action accion) { var b = new Button { Text = texto, Name = nombre, AutoSize = true, Padding = new Padding(4, 3, 4, 3), Margin = new Padding(4, 18, 4, 3) }; b.Click += delegate { Ejecutar(accion); }; return b; }
        private static Label Tarjeta(string nombre, string titulo) { return new Label { Name = nombre, Text = titulo + "\n—", Width = 165, Height = 56, Padding = new Padding(8, 5, 8, 5), Margin = new Padding(3), BackColor = TemaAplicacion.AcentoSuave, ForeColor = TemaAplicacion.Acento, Font = new Font("Segoe UI", 10, FontStyle.Bold) }; }
        private static Chart Grafico(string nombre, string titulo)
        {
            var chart = new Chart { Name = nombre, Dock = DockStyle.Fill, BackColor = Color.White };
            chart.Titles.Add(new Title(titulo, Docking.Top, new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Bold), Color.FromArgb(45, 55, 65)));
            var area = new ChartArea("Comparacion"); area.AxisX.MajorGrid.Enabled = false; area.AxisX.IsReversed = true;
            area.AxisX.Interval = 1; area.AxisX.LabelStyle.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8);
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro; area.AxisY.LabelStyle.Format = "0.#";
            chart.ChartAreas.Add(area);
            var serie = new Series("Proveedores") { ChartType = SeriesChartType.Bar, IsXValueIndexed = true, IsValueShownAsLabel = true, LabelFormat = "N1", Font = new Font(SystemFonts.DefaultFont.FontFamily, 8) };
            serie["BarLabelStyle"] = "Center"; chart.Series.Add(serie); return chart;
        }
        private void Ejecutar(Action accion)
        { try { UltimoError = null; UltimoMensaje = null; accion(); } catch (Exception e) { UltimoError = e.Message; if (!ModoPrueba) MessageBox.Show(this, e.Message, "Evaluación de proveedores", MessageBoxButtons.OK, MessageBoxIcon.Information); } }
        private void Informar(string texto) { UltimoMensaje = texto; if (!ModoPrueba) MessageBox.Show(this, texto, "Evaluación de proveedores", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        private int IdProducto { get { return producto.SelectedItem is Producto ? ((Producto)producto.SelectedItem).IdProducto : 0; } }

        public void ActualizarCatalogos()
        {
            int anterior = IdProducto; cargando = true;
            try { producto.DisplayMember = "Nombre"; producto.ValueMember = "IdProducto"; producto.DataSource = new ProductoBLL().Listar(true); if (producto.Items.Cast<Producto>().Any(p => p.IdProducto == anterior)) producto.SelectedValue = anterior; }
            finally { cargando = false; }
            if (actual != null && actual.Solicitud.IdProducto != IdProducto) Invalidar();
            CargarHistorial();
        }
        private void Actualizar()
        {
            var anterior = actual; var proveedor = Fila; string fundamento = motivo.Text; ActualizarCatalogos();
            if (anterior != null && anterior.Solicitud.IdProducto == IdProducto)
            {
                if (!anterior.IdProveedorSeleccionado.HasValue)
                {
                    Comparar();
                    if (proveedor != null) ElegirProveedor(proveedor.IdProveedor);
                    motivo.Text = fundamento;
                }
                else { Mostrar(bll.Obtener(anterior.IdEvaluacion)); estado.Text = "Decisión guardada. Pulse Comparar para evaluar los datos actuales."; }
            }
            else estado.Text = "Listas actualizadas a las " + DateTime.Now.ToString("HH:mm:ss") + ".";
        }
        private void CargarHistorial()
        {
            var anterior = historial.SelectedItem as ResumenEvaluacion;
            historial.ValueMember = "IdEvaluacion"; historial.DataSource = IdProducto > 0 ? bll.ConsultarHistorial(IdProducto) : null;
            if (anterior != null && historial.Items.Cast<ResumenEvaluacion>().Any(h => h.IdEvaluacion == anterior.IdEvaluacion)) historial.SelectedValue = anterior.IdEvaluacion;
        }
        private void Invalidar()
        {
            actual = null; proveedorSeleccionado = null; grilla.DataSource = null; motivo.Clear(); detalle.Text = "";
            puntajes.Series[0].Points.Clear(); margenes.Series[0].Points.Clear();
            stock.Text = "Stock utilizable\n—"; pendientes.Text = "Por recibir\n—"; ventas.Text = "Ventas en 90 días\n—"; reposicion.Text = "Reponer (30 días)\n—";
            contexto.Text = "Pulse Comparar para evaluar los datos actuales."; estado.Text = ""; ActualizarBotones();
        }
        private void Comparar()
        {
            Mostrar(bll.Evaluar(new SolicitudEvaluacion { IdProducto = IdProducto, Cantidad = (int)cantidad.Value, PesoPrecio = precio.Value, PesoPrestigio = prestigio.Value, PesoAntiguedad = antiguedad.Value }));
            CargarHistorial(); estado.Text = "Comparación actualizada a las " + DateTime.Now.ToString("HH:mm:ss") + ". Pulse una barra para elegir proveedor.";
            if (actual.Resultados.Count > 8) estado.Text += " Gráficos: primeros 8; listado: todos.";
        }
        private void UsarCantidadSugerida()
        {
            if (actual == null) { estado.Text = "Primero compare las ofertas para calcular la reposición sugerida."; return; }
            int sugerida = actual.CantidadSugerida;
            if (sugerida <= 0) { estado.Text = "No se sugiere reponer: el stock y las compras pendientes cubren el objetivo."; return; }
            if (actual.IdProveedorSeleccionado.HasValue) { estado.Text = "La decisión está guardada. Inicie otra comparación para cambiar la cantidad."; return; }
            cantidad.Value = sugerida; Comparar();
            estado.Text = "Se compararon las ofertas para la cantidad sugerida: " + sugerida + " u. Revise el proveedor antes de confirmar.";
        }
        private void Mostrar(EvaluacionProveedor e)
        {
            cargando = true;
            try { cantidad.Value = e.Solicitud.Cantidad; precio.Value = e.Solicitud.PesoPrecio; prestigio.Value = e.Solicitud.PesoPrestigio; antiguedad.Value = e.Solicitud.PesoAntiguedad; }
            finally { cargando = false; }
            actual = e; proveedorSeleccionado = null; grilla.BindingContext = BindingContext; grilla.DataSource = e.Resultados; motivo.Text = e.MotivoSeleccion ?? "";
            stock.Text = "Stock utilizable\n" + e.Datos.StockDisponible + " u."; pendientes.Text = "Por recibir\n" + e.Datos.ComprasPendientes + " u.";
            ventas.Text = "Ventas en 90 días\n" + e.Datos.UnidadesVendidas90Dias + " u."; reposicion.Text = "Reponer (30 días)\n" + e.CantidadSugerida + " u.";
            contexto.Text = e.IdProveedorSeleccionado.HasValue ? "Proveedor confirmado: " + e.Resultados.First(r => r.IdProveedor == e.IdProveedorSeleccionado).Proveedor :
                e.IdProveedorRecomendado.HasValue ? "Recomendado: " + e.Resultados.First(r => r.IdProveedor == e.IdProveedorRecomendado).Proveedor : "Sin recomendación. Consulte las ofertas excluidas en el listado.";
            contexto.Text += " · #" + e.IdEvaluacion + " del " + e.FechaEvaluacion.ToString("dd/MM/yyyy HH:mm");
            estado.Text = "Comparación guardada. Precio " + e.Solicitud.PesoPrecio + "% · Prestigio " + e.Solicitud.PesoPrestigio + "% · Antigüedad " + e.Solicitud.PesoAntiguedad + "%.";
            DibujarGraficos(e);
            int? elegir = e.IdProveedorSeleccionado ?? e.IdProveedorRecomendado ?? e.Resultados.Select(r => (int?)r.IdProveedor).FirstOrDefault();
            if (elegir.HasValue) ElegirProveedor(elegir.Value);
            MostrarDetalle(); ActualizarBotones();
        }
        private void DibujarGraficos(EvaluacionProveedor e)
        {
            puntajes.Series[0].Points.Clear(); margenes.Series[0].Points.Clear();
            puntajes.ChartAreas[0].AxisY.Minimum = 0; puntajes.ChartAreas[0].AxisY.Maximum = 100;
            foreach (var r in e.Resultados.Take(8))
            {
                string nombre = r.Proveedor.Length > 26 ? r.Proveedor.Substring(0, 25) + "…" : r.Proveedor;
                if (!r.Elegible) nombre += " (excl.)";
                var score = new DataPoint(0, (double)r.PuntajePonderadoFinal) { AxisLabel = nombre, Tag = r.IdProveedor,
                    Label = r.PuntajePonderadoFinal.ToString("N1", Cultura),
                    Color = !r.Elegible ? TemaAplicacion.Neutro : e.IdProveedorRecomendado == r.IdProveedor ? TemaAplicacion.Acento : Color.FromArgb(104, 151, 159),
                    LabelForeColor = r.Elegible && e.IdProveedorRecomendado == r.IdProveedor ? Color.White : TemaAplicacion.Texto,
                    ToolTip = r.Proveedor + "\n" + r.Fundamentos + "\nPrecio: " + r.PuntajePrecio.ToString("N1", Cultura) + " · Prestigio: " + r.PuntajePrestigio.ToString("N1", Cultura) + " · Antigüedad: " + r.PuntajeAntiguedad.ToString("N1", Cultura) };
                puntajes.Series[0].Points.Add(score);
                margenes.Series[0].Points.Add(new DataPoint(0, (double)r.MargenEstimado) { AxisLabel = nombre, Tag = r.IdProveedor,
                    Label = r.MargenEstimado.ToString("N1", Cultura),
                    Color = r.MargenEstimado <= 0 ? TemaAplicacion.Negativo : r.Elegible ? TemaAplicacion.Positivo : TemaAplicacion.Neutro,
                    LabelForeColor = r.MargenEstimado <= 0 || r.Elegible ? Color.White : TemaAplicacion.Texto,
                    ToolTip = r.Proveedor + "\nMargen: " + r.MargenEstimado.ToString("N1", Cultura) + "%\nBeneficio bruto estimado: " + r.BeneficioBrutoEstimado.ToString("C2", Cultura) });
            }
            if (e.Resultados.Count > 8) estado.Text += " Gráficos: primeros 8; el listado incluye todos.";
            margenes.ChartAreas[0].RecalculateAxesScale(); puntajes.Invalidate(); margenes.Invalidate();
        }
        private void ElegirDesdeGrafico(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var hit = ((Chart)sender).HitTest(e.X, e.Y, false, ChartElementType.DataPoint).FirstOrDefault(h => h.Series != null && h.PointIndex >= 0);
            if (hit != null) ElegirProveedor((int)hit.Series.Points[hit.PointIndex].Tag);
        }
        private void ElegirProveedor(int id)
        {
            if (actual == null || !actual.Resultados.Any(r => r.IdProveedor == id)) return;
            foreach (DataGridViewRow row in grilla.Rows) if (((ResultadoProveedor)row.DataBoundItem).IdProveedor == id) { grilla.CurrentCell = row.Cells[0]; break; }
            proveedorSeleccionado = id; MostrarDetalle();
        }
        private ResultadoProveedor Fila { get { return actual == null ? null : actual.Resultados.FirstOrDefault(r => r.IdProveedor == proveedorSeleccionado); } }
        private void MostrarDetalle()
        {
            var r = Fila;
            detalle.Text = r == null ? "" : r.Proveedor + " · " + (r.Elegible ? "Elegible" : r.Fundamentos) + " · Entrega: " + r.PlazoEntregaDias + " días · Beneficio bruto: " + r.BeneficioBrutoEstimado.ToString("C2", Cultura) + Environment.NewLine + r.Advertencias.Trim();
            foreach (var chart in new[] { puntajes, margenes })
                foreach (var punto in chart.Series[0].Points)
                { punto.BorderWidth = r != null && (int)punto.Tag == r.IdProveedor ? 2 : 0; punto.BorderColor = Color.FromArgb(35, 45, 55); }
            ActualizarBotones();
        }
        private void ActualizarBotones()
        {
            if (confirmar == null) return;
            motivo.ReadOnly = actual != null && actual.IdProveedorSeleccionado.HasValue;
            confirmar.Enabled = actual != null && !actual.IdProveedorSeleccionado.HasValue && Fila != null && Fila.Elegible;
            crear.Enabled = actual != null && actual.IdProveedorSeleccionado.HasValue && (!actual.IdOrdenCompra.HasValue || AbrirOrdenSolicitada != null);
            crear.Text = actual != null && actual.IdOrdenCompra.HasValue ? "Abrir orden #" + actual.IdOrdenCompra : "Crear orden en Borrador";
        }
        private void Confirmar()
        { if (actual == null || Fila == null) return; bll.SeleccionarPrioritario(actual.IdEvaluacion, Fila.IdProveedor, motivo.Text); Mostrar(bll.Obtener(actual.IdEvaluacion)); CargarHistorial(); Informar("Proveedor confirmado. Puede iniciar la compra con un borrador."); }
        private void CrearOrden()
        {
            if (actual == null) return;
            if (actual.IdOrdenCompra.HasValue) { if (AbrirOrdenSolicitada != null) AbrirOrdenSolicitada(actual.IdOrdenCompra.Value); return; }
            int id = bll.CrearOrdenBorrador(actual.IdEvaluacion); Mostrar(bll.Obtener(actual.IdEvaluacion)); CargarHistorial();
            estado.Text = "Orden #" + id + " creada. Pulse Abrir orden para continuar con la revisión de Contabilidad.";
        }
    }
}
