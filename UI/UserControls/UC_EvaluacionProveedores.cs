using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace UI.UserControls
{
    public class UC_EvaluacionProveedores : UserControl
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
        private readonly EvaluacionProveedorBLL bll = new EvaluacionProveedorBLL();
        private readonly ComboBox producto = new ComboBox { Name = "cmbEvaluacionProducto", Width = 270, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown cantidad = Numero("nudEvaluacionCantidad", 1, 100000, 10);
        private readonly NumericUpDown precio = Numero("nudPesoPrecio", 0, 100, 50);
        private readonly NumericUpDown prestigio = Numero("nudPesoPrestigio", 0, 100, 30);
        private readonly NumericUpDown antiguedad = Numero("nudPesoAntiguedad", 0, 100, 20);
        private readonly DataGridView grilla = new DataGridView { Name = "dgvEvaluacion", Dock = DockStyle.Fill, ReadOnly = true,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoGenerateColumns = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private readonly Label contexto = new Label { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4), Text = "Seleccione un perfume y pulse Comparar. Las cotizaciones se registran en Catálogos." };
        private readonly TextBox explicacion = new TextBox { Name = "txtFundamentosEvaluacion", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
        private readonly TextBox motivo = new TextBox { Name = "txtMotivoSeleccion", Width = 410, MaxLength = 500 };
        private readonly ComboBox historial = new ComboBox { Name = "cmbHistorialEvaluacion", Width = 450, FormattingEnabled = true, DropDownStyle = ComboBoxStyle.DropDownList };
        private Button confirmar, crear;
        private EvaluacionProveedor actual;
        private bool cargando;
        public bool ModoPrueba { get; set; }
        public string UltimoError { get; private set; }
        public string UltimoMensaje { get; private set; }

        public UC_EvaluacionProveedores()
        {
            Dock = DockStyle.Fill;
            BindingContext = new BindingContext();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, Padding = new Padding(10) };
            foreach (var style in new[] { new RowStyle(SizeType.AutoSize), new RowStyle(SizeType.AutoSize), new RowStyle(SizeType.AutoSize), new RowStyle(SizeType.Absolute, 76), new RowStyle(SizeType.Percent, 100), new RowStyle(SizeType.Absolute, 95), new RowStyle(SizeType.AutoSize), new RowStyle(SizeType.AutoSize) }) layout.RowStyles.Add(style);
            layout.Controls.Add(new Label { Text = "N03 · Evaluación y elección de proveedores", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(3, 0, 0, 10) }, 0, 0);
            var filtros = Flujo(); Campo(filtros, "Perfume", producto); Campo(filtros, "Cantidad a comprar", cantidad);
            filtros.Controls.Add(Boton("Comparar", "btnCompararProveedores", Comparar));
            var criterios = Flujo(); criterios.Visible = false;
            Campo(criterios, "Precio %", precio); Campo(criterios, "Prestigio %", prestigio); Campo(criterios, "Antigüedad %", antiguedad);
            criterios.Controls.Add(new Label { Text = "Deben sumar 100. Antigüedad: máximo 20 años para el puntaje.", AutoSize = true, Margin = new Padding(8) });
            filtros.Controls.Add(Boton("Criterios", "btnCriteriosEvaluacion", delegate { criterios.Visible = !criterios.Visible; }));
            layout.Controls.Add(filtros, 0, 1); layout.Controls.Add(criterios, 0, 2); layout.Controls.Add(contexto, 0, 3);
            foreach (var col in new[] { new[] { "Proveedor", "Proveedor", "210" }, new[] { "PuntajePonderadoFinal", "Puntaje / 100", "100" }, new[] { "PrecioUnitario", "Precio unitario", "110" }, new[] { "CostoTotal", "Costo de la compra", "130" }, new[] { "MargenEstimado", "Margen bruto %", "110" }, new[] { "PlazoEntregaDias", "Plazo (días)", "95" }, new[] { "Elegible", "Elegible", "70" } })
                grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = col[0], HeaderText = col[1], Width = int.Parse(col[2]), DefaultCellStyle = new DataGridViewCellStyle { FormatProvider = Cultura, Format = col[0] == "PrecioUnitario" || col[0] == "CostoTotal" ? "N2" : col[0] == "MargenEstimado" || col[0] == "PuntajePonderadoFinal" ? "N2" : "" } });
            grilla.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e) { if (grilla.Columns[e.ColumnIndex].DataPropertyName == "Elegible" && e.Value is bool) { e.Value = (bool)e.Value ? "Sí" : "No"; e.FormattingApplied = true; } };
            grilla.SelectionChanged += delegate { MostrarDetalle(); };
            layout.Controls.Add(grilla, 0, 4); layout.Controls.Add(explicacion, 0, 5);
            var decision = Flujo(); Campo(decision, "Fundamento de la elección (mínimo 10 caracteres)", motivo);
            confirmar = Boton("Confirmar proveedor", "btnConfirmarProveedor", Confirmar);
            crear = Boton("Crear orden en Borrador", "btnOrdenEvaluacion", CrearOrden);
            decision.Controls.Add(confirmar); decision.Controls.Add(crear); layout.Controls.Add(decision, 0, 6);
            var pie = Flujo(); Campo(pie, "Comparaciones guardadas", historial);
            historial.Format += delegate(object sender, ListControlConvertEventArgs e) { var r = (ResumenEvaluacion)e.ListItem; e.Value = "#" + r.IdEvaluacion + " · " + r.Fecha.ToString("dd/MM HH:mm") + " · " + r.ProveedorSeleccionado; };
            pie.Controls.Add(Boton("Abrir", "btnAbrirEvaluacion", delegate { Ejecutar(delegate { var h = historial.SelectedItem as ResumenEvaluacion; if (h != null) Mostrar(bll.Obtener(h.IdEvaluacion)); }); }));
            pie.Controls.Add(Boton("Actualizar", "btnActualizarEvaluacion", delegate { Ejecutar(CargarProductos); }));
            layout.Controls.Add(pie, 0, 7); Controls.Add(layout);
            producto.SelectedIndexChanged += delegate { if (!cargando) { Invalidar(); Ejecutar(CargarHistorial); } };
            foreach (var n in new[] { cantidad, precio, prestigio, antiguedad }) n.ValueChanged += delegate { if (!cargando) Invalidar(); };
            ActualizarBotones();
            Ejecutar(CargarProductos);
        }
        private static NumericUpDown Numero(string nombre, int min, int max, int value) { return new NumericUpDown { Name = nombre, Minimum = min, Maximum = max, Value = value, Width = 85 }; }
        private static FlowLayoutPanel Flujo() { return new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(2) }; }
        private static void Campo(FlowLayoutPanel panel, string etiqueta, Control control)
        { var campo = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(3, 2, 8, 4) }; campo.Controls.Add(new Label { Text = etiqueta, AutoSize = true }); campo.Controls.Add(control); panel.Controls.Add(campo); }
        private Button Boton(string texto, string nombre, Action accion) { var b = new Button { Text = texto, Name = nombre, AutoSize = true, Height = 30, Margin = new Padding(4, 19, 4, 4) }; b.Click += delegate { Ejecutar(accion); }; return b; }
        private void Ejecutar(Action accion)
        { try { UltimoError = null; UltimoMensaje = null; accion(); } catch (Exception e) { UltimoError = e.Message; if (!ModoPrueba) MessageBox.Show(this, e.Message, "Evaluación de proveedores", MessageBoxButtons.OK, MessageBoxIcon.Information); } }
        private void Informar(string texto) { UltimoMensaje = texto; if (!ModoPrueba) MessageBox.Show(this, texto, "Evaluación de proveedores", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        private int IdProducto { get { return producto.SelectedItem is Producto ? ((Producto)producto.SelectedItem).IdProducto : 0; } }
        private void CargarProductos()
        { int anterior = IdProducto; cargando = true; try { producto.DisplayMember = "Nombre"; producto.ValueMember = "IdProducto"; producto.DataSource = new ProductoBLL().Listar(true); if (anterior > 0) producto.SelectedValue = anterior; } finally { cargando = false; } Invalidar(); CargarHistorial(); }
        private void CargarHistorial() { historial.DataSource = IdProducto > 0 ? bll.ConsultarHistorial(IdProducto) : null; }
        private void Invalidar() { actual = null; grilla.DataSource = null; explicacion.Clear(); contexto.Text = "Pulse Comparar para evaluar con los datos actuales."; ActualizarBotones(); }
        private void Comparar()
        { Mostrar(bll.Evaluar(new SolicitudEvaluacion { IdProducto = IdProducto, Cantidad = (int)cantidad.Value, PesoPrecio = precio.Value, PesoPrestigio = prestigio.Value, PesoAntiguedad = antiguedad.Value })); CargarHistorial(); }
        private void Mostrar(EvaluacionProveedor e)
        {
            cargando = true;
            try { cantidad.Value = e.Solicitud.Cantidad; precio.Value = e.Solicitud.PesoPrecio; prestigio.Value = e.Solicitud.PesoPrestigio; antiguedad.Value = e.Solicitud.PesoAntiguedad; }
            finally { cargando = false; }
            actual = e; grilla.DataSource = e.Resultados; motivo.Text = e.MotivoSeleccion ?? "";
            contexto.Text = "Evaluación #" + e.IdEvaluacion + " · " + e.FechaEvaluacion.ToString("dd/MM/yyyy HH:mm") + " · Precio " + e.Solicitud.PesoPrecio + "% / prestigio " + e.Solicitud.PesoPrestigio + "% / antigüedad " + e.Solicitud.PesoAntiguedad + "%\n" +
                "Stock utilizable: " + e.Datos.StockDisponible + " · Pendiente de recibir: " + e.Datos.ComprasPendientes + " · Ventas 90 días: " + e.Datos.UnidadesVendidas90Dias + " u. · Reposición sugerida (30 días): " + e.CantidadSugerida + " u.\n" +
                "Precio de venta de referencia: $ " + e.PrecioVentaReferencia.ToString("N2", Cultura) + " · " + (e.IdProveedorRecomendado.HasValue ? "Recomendado: " + e.Resultados.First(r => r.IdProveedor == e.IdProveedorRecomendado).Proveedor : "Sin recomendación: revise las ofertas excluidas.");
            MostrarDetalle(); ActualizarBotones();
            if (e.IdProveedorSeleccionado.HasValue)
                contexto.Text += " · Confirmado: " + e.Resultados.First(r => r.IdProveedor == e.IdProveedorSeleccionado).Proveedor + " por " + e.SeleccionadaPor + (e.IdOrdenCompra.HasValue ? " · Orden #" + e.IdOrdenCompra : "");
        }
        private ResultadoProveedor Fila { get { return grilla.CurrentRow == null ? null : grilla.CurrentRow.DataBoundItem as ResultadoProveedor; } }
        private void MostrarDetalle()
        {
            var r = Fila;
            explicacion.Text = r == null ? "" : r.Fundamentos + Environment.NewLine + "Precio: " + r.PuntajePrecio.ToString("N2", Cultura) + "/100; prestigio: " + r.PuntajePrestigio.ToString("N2", Cultura) + "/100; antigüedad: " + r.PuntajeAntiguedad.ToString("N2", Cultura) + "/100. Beneficio bruto estimado: $ " + r.BeneficioBrutoEstimado.ToString("N2", Cultura) + "." + Environment.NewLine + r.Advertencias;
            ActualizarBotones();
        }
        private void ActualizarBotones() { if (confirmar == null) return; confirmar.Enabled = actual != null && !actual.IdProveedorSeleccionado.HasValue && Fila != null && Fila.Elegible; crear.Enabled = actual != null && actual.IdProveedorSeleccionado.HasValue; }
        private void Confirmar()
        { if (actual == null || Fila == null) return; bll.SeleccionarPrioritario(actual.IdEvaluacion, Fila.IdProveedor, motivo.Text); Mostrar(bll.Obtener(actual.IdEvaluacion)); CargarHistorial(); Informar("Proveedor confirmado. Puede crear una orden en Borrador para continuar el circuito contable habitual."); }
        private void CrearOrden()
        { if (actual == null) return; int id = bll.CrearOrdenBorrador(actual.IdEvaluacion); Mostrar(bll.Obtener(actual.IdEvaluacion)); CargarHistorial(); Informar("Orden en Borrador: " + id + ". Se conserva la revisión y aprobación de Contabilidad."); }
    }
}
