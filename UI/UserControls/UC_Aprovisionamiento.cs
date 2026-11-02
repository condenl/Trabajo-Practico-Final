using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BE.Seguridad;
using BLL;
using Servicios;
using UI.Reportes;

namespace UI.UserControls
{
    public class UC_Aprovisionamiento : UserControl
    {
        public bool ModoPrueba { get; set; }
        public string UltimoMensaje { get; private set; }
        public string UltimoError { get; private set; }
        public string RutaArchivoPrueba { get; set; }
        public bool ConfirmarImportacionPrueba { get; set; }

        private readonly Usuario usuario;
        private readonly ProveedorBLL proveedorBLL = new ProveedorBLL();
        private readonly ProductoBLL productoBLL = new ProductoBLL();
        private readonly AprovisionamientoBLL aprovisionamientoBLL = new AprovisionamientoBLL();
        private readonly InventarioBLL inventarioBLL = new InventarioBLL();
        private readonly IntercambioOrdenesBLL intercambioOrdenesBLL = new IntercambioOrdenesBLL();
        private Button btnReporteOrdenesPdf;
        private Button btnExportarOrdenesXml;

        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly TabPage tabCatalogos = new TabPage("Catálogos y cotizaciones");
        private readonly TabPage tabOrdenes = new TabPage("Órdenes de compra");
        private readonly TabPage tabContabilidad = new TabPage("Aprobación contable");
        private readonly TabPage tabRecepcion = new TabPage("Recepción e inventario");

        private readonly TextBox txtProveedorRazon = new TextBox { Name = "txtProveedorRazon" };
        private readonly TextBox txtProveedorCuit = new TextBox { Name = "txtProveedorCuit" };
        private readonly TextBox txtProveedorContacto = new TextBox { Name = "txtProveedorContacto" };
        private readonly NumericUpDown nudProveedorAntiguedad = new NumericUpDown { Name = "nudProveedorAntiguedad", Maximum = 200 };
        private readonly NumericUpDown nudProveedorPrestigio = new NumericUpDown { Name = "nudProveedorPrestigio", Maximum = 100, DecimalPlaces = 2 };

        private readonly TextBox txtProductoCodigo = new TextBox { Name = "txtProductoCodigo" };
        private readonly TextBox txtProductoNombre = new TextBox { Name = "txtProductoNombre" };
        private readonly TextBox txtProductoMarca = new TextBox { Name = "txtProductoMarca" };
        private readonly TextBox txtProductoFamilia = new TextBox { Name = "txtProductoFamilia" };
        private readonly NumericUpDown nudProductoPrecio = Dinero("nudProductoPrecio");
        private readonly NumericUpDown nudProductoStockMinimo = new NumericUpDown { Name = "nudProductoStockMinimo", Maximum = 100000 };

        private readonly ComboBox cmbCotizacionProveedor = Combo("cmbCotizacionProveedor");
        private readonly ComboBox cmbCotizacionProducto = Combo("cmbCotizacionProducto");
        private readonly NumericUpDown nudCotizacionPrecio = Dinero("nudCotizacionPrecio");
        private readonly NumericUpDown nudCotizacionPlazo = new NumericUpDown { Name = "nudCotizacionPlazo", Maximum = 365 };
        private readonly DateTimePicker dtpCotizacionVigencia = new DateTimePicker { Name = "dtpCotizacionVigencia", Value = DateTime.Today.AddMonths(1) };
        private readonly TextBox txtCotizacionCondiciones = new TextBox { Name = "txtCotizacionCondiciones" };

        private readonly ComboBox cmbOrdenProveedor = Combo("cmbOrdenProveedor");
        private readonly ComboBox cmbOrdenProducto = Combo("cmbOrdenProducto");
        private readonly NumericUpDown nudOrdenCantidad = new NumericUpDown { Name = "nudOrdenCantidad", Minimum = 1, Maximum = 100000, Value = 1 };
        private readonly NumericUpDown nudOrdenPrecio = Dinero("nudOrdenPrecio");
        private readonly DataGridView dgvOrdenes = Grilla("dgvOrdenes");

        private readonly DataGridView dgvPendientes = Grilla("dgvPendientes");
        private readonly TextBox txtMotivoRechazo = new TextBox { Name = "txtMotivoRechazo" };

        private readonly ComboBox cmbOrdenRecepcion = Combo("cmbOrdenRecepcion");
        private readonly ComboBox cmbRecepcionProducto = Combo("cmbRecepcionProducto");
        private readonly TextBox txtLoteCodigo = new TextBox { Name = "txtLoteCodigo" };
        private readonly TextBox txtLoteOrigen = new TextBox { Name = "txtLoteOrigen" };
        private readonly NumericUpDown nudLoteCantidad = new NumericUpDown { Name = "nudLoteCantidad", Minimum = 1, Maximum = 100000, Value = 1 };
        private readonly DateTimePicker dtpLoteVencimiento = new DateTimePicker { Name = "dtpLoteVencimiento", Value = DateTime.Today.AddYears(2), ShowCheckBox = true, Checked = true };
        private readonly ComboBox cmbInventarioProducto = Combo("cmbInventarioProducto");
        private readonly Label lblStock = new Label { Name = "lblStock", AutoSize = true, Text = "Stock disponible: -" };
        private readonly DataGridView dgvTrazabilidad = Grilla("dgvTrazabilidad");

        public UC_Aprovisionamiento()
            : this(Session.Instance.Usuario)
        {
        }

        public UC_Aprovisionamiento(Usuario usuarioActual)
        {
            usuario = usuarioActual;
            Dock = DockStyle.Fill;
            ConstruirInterfaz();
            ConfigurarAcceso();
            RecargarTodo();
        }

        private static ComboBox Combo(string nombre)
        {
            return new ComboBox { Name = nombre, DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        }

        private static NumericUpDown Dinero(string nombre)
        {
            return new NumericUpDown { Name = nombre, DecimalPlaces = 2, Maximum = 100000000, Minimum = 0, ThousandsSeparator = true };
        }

        private static DataGridView Grilla(string nombre)
        {
            return new DataGridView
            {
                Name = nombre,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
        }

        private void ConstruirInterfaz()
        {
            tabs.TabPages.Add(tabCatalogos);
            tabs.TabPages.Add(tabOrdenes);
            tabs.TabPages.Add(tabContabilidad);
            tabs.TabPages.Add(tabRecepcion);
            ConstruirCatalogos();
            ConstruirOrdenes();
            ConstruirContabilidad();
            ConstruirRecepcion();
            if (TienePermiso(TipoPermiso.Aprovisionamiento_Gestion))
            {
                var evaluacion = new TabPage("Evaluación de proveedores");
                evaluacion.Controls.Add(new UC_EvaluacionProveedores());
                tabs.TabPages.Add(evaluacion);
            }
            Controls.Add(tabs);
        }

        private void ConstruirCatalogos()
        {
            FlowLayoutPanel flujo = FlujoVertical();
            Label titulo = Titulo("Aprovisionamiento: catálogos y cotizaciones comparables");
            flujo.Controls.Add(titulo);

            GroupBox proveedor = Grupo("Registrar proveedor", 1180, 115);
            FlowLayoutPanel fp = FlujoHorizontal();
            AgregarCampo(fp, "Razón social", txtProveedorRazon, 180);
            AgregarCampo(fp, "CUIT", txtProveedorCuit, 120);
            AgregarCampo(fp, "Contacto", txtProveedorContacto, 180);
            AgregarCampo(fp, "Antigüedad (años)", nudProveedorAntiguedad, 90);
            AgregarCampo(fp, "Prestigio (0-100)", nudProveedorPrestigio, 90);
            Button guardarProveedor = Boton("Guardar proveedor", "btnGuardarProveedor", GuardarProveedor);
            fp.Controls.Add(guardarProveedor);
            proveedor.Controls.Add(fp);
            flujo.Controls.Add(proveedor);

            GroupBox producto = Grupo("Registrar perfume", 1180, 115);
            FlowLayoutPanel fprod = FlujoHorizontal();
            AgregarCampo(fprod, "Código", txtProductoCodigo, 90);
            AgregarCampo(fprod, "Nombre", txtProductoNombre, 180);
            AgregarCampo(fprod, "Marca", txtProductoMarca, 130);
            AgregarCampo(fprod, "Familia olfativa", txtProductoFamilia, 130);
            AgregarCampo(fprod, "Precio venta", nudProductoPrecio, 100);
            AgregarCampo(fprod, "Stock mínimo", nudProductoStockMinimo, 90);
            fprod.Controls.Add(Boton("Guardar perfume", "btnGuardarProducto", GuardarProducto));
            producto.Controls.Add(fprod);
            flujo.Controls.Add(producto);

            GroupBox cotizacion = Grupo("Registrar cotización recibida", 1180, 125);
            FlowLayoutPanel fc = FlujoHorizontal();
            AgregarCampo(fc, "Proveedor", cmbCotizacionProveedor, 200);
            AgregarCampo(fc, "Perfume", cmbCotizacionProducto, 200);
            AgregarCampo(fc, "Precio unitario", nudCotizacionPrecio, 100);
            AgregarCampo(fc, "Entrega (días)", nudCotizacionPlazo, 80);
            AgregarCampo(fc, "Vigencia", dtpCotizacionVigencia, 135);
            AgregarCampo(fc, "Condiciones", txtCotizacionCondiciones, 160);
            fc.Controls.Add(Boton("Registrar cotización", "btnRegistrarCotizacion", RegistrarCotizacion));
            cotizacion.Controls.Add(fc);
            flujo.Controls.Add(cotizacion);
            tabCatalogos.Controls.Add(flujo);
        }

        private void ConstruirOrdenes()
        {
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(Titulo("Orden de compra y envío a revisión contable"), 0, 0);
            FlowLayoutPanel barra = FlujoHorizontal();
            AgregarCampo(barra, "Proveedor", cmbOrdenProveedor, 210);
            AgregarCampo(barra, "Perfume", cmbOrdenProducto, 210);
            AgregarCampo(barra, "Cantidad", nudOrdenCantidad, 85);
            AgregarCampo(barra, "Precio compra", nudOrdenPrecio, 105);
            barra.Controls.Add(Boton("Crear borrador", "btnCrearOrden", CrearOrden));
            barra.Controls.Add(Boton("Enviar a Contabilidad", "btnEnviarRevision", EnviarRevision));
            barra.Controls.Add(Boton("Emitir al proveedor", "btnEmitirOrden", EmitirOrden));
            barra.Controls.Add(Boton("Actualizar", "btnActualizarOrdenes", delegate { CargarOrdenes(); }));
            layout.Controls.Add(barra, 0, 1);
            var archivos = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(5) };
            btnReporteOrdenesPdf = Boton("Reporte PDF", "btnReporteOrdenesPdf", GenerarReportePdf);
            btnExportarOrdenesXml = Boton("Exportar XML", "btnExportarOrdenesXml", ExportarOrdenesXml);
            var importar = Boton("Importar XML", "btnImportarOrdenesXml", ImportarOrdenesXml);
            var ejemplo = Boton("Sample XML", "btnSampleOrdenesXml", CopiarXmlEjemplo);
            foreach (Button boton in new[] { btnReporteOrdenesPdf, btnExportarOrdenesXml, importar, ejemplo })
            {
                boton.Margin = new Padding(3);
                archivos.Controls.Add(boton);
            }
            archivos.Controls.Add(new Label { AutoSize = true, MaximumSize = new Size(430, 0), Margin = new Padding(10, 9, 3, 3),
                Text = "PDF/XML: listado completo con detalles. Importar crea borradores nuevos." });
            layout.Controls.Add(archivos, 0, 2);
            layout.Controls.Add(dgvOrdenes, 0, 3);
            tabOrdenes.Controls.Add(layout);
        }

        private void ConstruirContabilidad()
        {
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(Titulo("Revisión y aprobación presupuestaria por Contabilidad"), 0, 0);
            FlowLayoutPanel barra = FlujoHorizontal();
            barra.Controls.Add(Boton("Aprobar seleccionada", "btnAprobarOrden", AprobarOrden));
            AgregarCampo(barra, "Motivo de rechazo", txtMotivoRechazo, 330);
            barra.Controls.Add(Boton("Rechazar seleccionada", "btnRechazarOrden", RechazarOrden));
            barra.Controls.Add(Boton("Actualizar", "btnActualizarPendientes", delegate { CargarPendientes(); }));
            layout.Controls.Add(barra, 0, 1);
            layout.Controls.Add(dgvPendientes, 0, 2);
            tabContabilidad.Controls.Add(layout);
        }

        private void ConstruirRecepcion()
        {
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(Titulo("Recepción con lote, origen y trazabilidad"), 0, 0);
            FlowLayoutPanel recepcion = FlujoHorizontal();
            AgregarCampo(recepcion, "Orden emitida", cmbOrdenRecepcion, 180);
            cmbOrdenRecepcion.SelectedIndexChanged += CambiarOrdenRecepcion;
            AgregarCampo(recepcion, "Perfume", cmbRecepcionProducto, 200);
            AgregarCampo(recepcion, "Código lote", txtLoteCodigo, 110);
            AgregarCampo(recepcion, "Origen", txtLoteOrigen, 130);
            AgregarCampo(recepcion, "Cantidad", nudLoteCantidad, 80);
            AgregarCampo(recepcion, "Vencimiento", dtpLoteVencimiento, 135);
            recepcion.Controls.Add(Boton("Registrar recepción", "btnRegistrarRecepcion", RegistrarRecepcion));
            layout.Controls.Add(recepcion, 0, 1);
            layout.Controls.Add(Titulo("Consulta de stock y trazabilidad"), 0, 2);
            FlowLayoutPanel consulta = FlujoHorizontal();
            AgregarCampo(consulta, "Perfume", cmbInventarioProducto, 240);
            consulta.Controls.Add(Boton("Consultar", "btnConsultarInventario", ConsultarInventario));
            consulta.Controls.Add(lblStock);
            layout.Controls.Add(consulta, 0, 3);
            layout.Controls.Add(dgvTrazabilidad, 0, 4);
            tabRecepcion.Controls.Add(layout);
        }

        private void ConfigurarAcceso()
        {
            bool gestiona = TienePermiso(TipoPermiso.Aprovisionamiento_Gestion);
            bool aprueba = TienePermiso(TipoPermiso.Contabilidad_AprobarCompras);
            bool recibe = TienePermiso(TipoPermiso.Inventario_Recepcionar);
            if (!gestiona) { tabs.TabPages.Remove(tabCatalogos); tabs.TabPages.Remove(tabOrdenes); }
            if (!aprueba) tabs.TabPages.Remove(tabContabilidad);
            if (!recibe) tabs.TabPages.Remove(tabRecepcion);
        }

        private bool TienePermiso(TipoPermiso permiso)
        {
            return usuario != null && usuario.TienePermiso(permiso);
        }

        private void RecargarTodo()
        {
            try
            {
                CargarCatalogos();
                CargarOrdenes();
                CargarPendientes();
                CargarOrdenesRecepcion();
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void CargarCatalogos()
        {
            List<Proveedor> proveedores = proveedorBLL.Listar(true);
            List<Producto> productos = productoBLL.Listar(true);
            Enlazar(cmbCotizacionProveedor, proveedores, "RazonSocial", "IdProveedor");
            Enlazar(cmbOrdenProveedor, proveedores.ToList(), "RazonSocial", "IdProveedor");
            Enlazar(cmbCotizacionProducto, productos, "Nombre", "IdProducto");
            Enlazar(cmbOrdenProducto, productos.ToList(), "Nombre", "IdProducto");
            Enlazar(cmbInventarioProducto, productos.ToList(), "Nombre", "IdProducto");
        }

        private static void Enlazar(ComboBox combo, object datos, string mostrar, string valor)
        {
            combo.DataSource = null;
            combo.DisplayMember = mostrar;
            combo.ValueMember = valor;
            combo.DataSource = datos;
        }

        private void CargarOrdenes()
        {
            if (!TienePermiso(TipoPermiso.Aprovisionamiento_Gestion)) return;
            MostrarOrdenes(dgvOrdenes, aprovisionamientoBLL.ConsultarHistorialCompras(null));
            btnReporteOrdenesPdf.Enabled = btnExportarOrdenesXml.Enabled = dgvOrdenes.Rows.Count > 0;
        }

        private void MostrarOrdenes(DataGridView grilla, IEnumerable<OrdenCompra> ordenes)
        {
            // Include inactive catalog entries so historical orders keep their names.
            var proveedores = proveedorBLL.Listar(false).ToDictionary(p => p.IdProveedor);
            var productos = productoBLL.Listar(false).ToDictionary(p => p.IdProducto);
            grilla.DataSource = ordenes.Select(o => OrdenCompraVista.Crear(o, proveedores, productos)).ToList();
            grilla.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grilla.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grilla.Columns["IdOrdenCompra"].HeaderText = "Orden";
            grilla.Columns["IdOrdenCompra"].FillWeight = 50;
            grilla.Columns["Proveedor"].FillWeight = 140;
            grilla.Columns["Productos"].HeaderText = "Productos y cantidades";
            grilla.Columns["Productos"].FillWeight = 240;
            grilla.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
            grilla.Columns["Total"].DefaultCellStyle.Format = "N2";
            grilla.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grilla.Columns["CreadaPor"].HeaderText = "Responsable";
            grilla.Columns["Unidades"].HeaderText = "Cantidad total";
            grilla.Columns["Unidades"].FillWeight = 65;
        }

        private ArchivoOrdenesCompra ObtenerListadoGrilla()
        {
            var ids = dgvOrdenes.Rows.Cast<DataGridViewRow>().Where(f => !f.IsNewRow && f.Visible)
                .Select(f => ((OrdenCompraVista)f.DataBoundItem).IdOrdenCompra).ToList();
            return intercambioOrdenesBLL.ObtenerListado(ids);
        }

        private string ElegirArchivo(bool importar, bool pdf = false)
        {
            if (ModoPrueba)
            {
                if (string.IsNullOrWhiteSpace(RutaArchivoPrueba)) throw new InvalidOperationException("Debe indicar RutaArchivoPrueba.");
                return RutaArchivoPrueba;
            }
            if (importar)
            {
                using (var dialogo = new OpenFileDialog { Title = "Importar órdenes desde XML", Filter = "Órdenes XML (*.xml)|*.xml",
                    CheckFileExists = true, Multiselect = false })
                    return dialogo.ShowDialog(this) == DialogResult.OK ? dialogo.FileName : null;
            }
            using (var dialogo = new SaveFileDialog { Title = pdf ? "Guardar reporte de órdenes" : "Exportar listado de órdenes",
                Filter = pdf ? "Reporte PDF (*.pdf)|*.pdf" : "Órdenes XML (*.xml)|*.xml", DefaultExt = pdf ? "pdf" : "xml",
                AddExtension = true, CheckPathExists = true, OverwritePrompt = true,
                FileName = "ordenes_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + (pdf ? ".pdf" : ".xml") })
                return dialogo.ShowDialog(this) == DialogResult.OK ? dialogo.FileName : null;
        }

        private void GenerarReportePdf(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                ArchivoOrdenesCompra archivo = ObtenerListadoGrilla();
                string ruta = ElegirArchivo(false, true);
                if (ruta == null) return;
                ReporteOrdenesPdf.Guardar(ruta, archivo);
                Informar("Reporte PDF guardado. Órdenes incluidas: " + archivo.Ordenes.Count + ".");
            }, null);
        }

        private void ExportarOrdenesXml(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                ArchivoOrdenesCompra archivo = ObtenerListadoGrilla();
                string ruta = ElegirArchivo(false);
                if (ruta == null) return;
                OrdenesXml.Guardar(ruta, archivo);
                Informar("XML guardado con " + archivo.Ordenes.Count + " órdenes y sus detalles.");
            }, null);
        }

        private void ImportarOrdenesXml(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                string ruta = ElegirArchivo(true);
                if (ruta == null) return;
                ArchivoOrdenesCompra archivo = OrdenesXml.Leer(ruta);
                int cantidad = intercambioOrdenesBLL.ValidarImportacion(archivo);
                bool confirmar = ModoPrueba ? ConfirmarImportacionPrueba : MessageBox.Show(this,
                    "Se crearán " + cantidad + " órdenes NUEVAS en Borrador, con fecha y responsable actuales.\n" +
                    "No se copian aprobaciones, recepciones ni stock y no se reemplazan órdenes existentes.\n" +
                    "Volver a importar el mismo archivo crea otras copias.\n\n¿Desea continuar?",
                    "Confirmar importación XML", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
                if (!confirmar) return;
                List<int> ids = intercambioOrdenesBLL.Importar(archivo);
                CargarOrdenes();
                Informar("Se importaron " + ids.Count + " órdenes como borradores. Nuevos IDs: " +
                    string.Join(", ", ids.Take(10)) + (ids.Count > 10 ? "..." : "") + ".");
            }, null);
        }

        private void CopiarXmlEjemplo(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                string xml = OrdenesXml.Serializar(intercambioOrdenesBLL.CrearEjemplo());
                Clipboard.SetDataObject(xml, true, 5, 100);
                Informar("Se copió al portapapeles un XML con 3 órdenes de ejemplo y precios ilustrativos.\n" +
                    "Pegue el contenido en un archivo .xml guardado como UTF-8 y ábralo con Importar XML.\n" +
                    "Todavía no se creó ninguna orden.");
            }, null);
        }

        private void CargarPendientes()
        {
            if (!TienePermiso(TipoPermiso.Contabilidad_AprobarCompras)) return;
            MostrarOrdenes(dgvPendientes, aprovisionamientoBLL.ConsultarHistorialCompras(null)
                .Where(o => o.Estado == EstadoOrdenCompra.PendienteAprobacion));
        }

        private void CargarOrdenesRecepcion()
        {
            if (!TienePermiso(TipoPermiso.Inventario_Recepcionar)) return;
            List<OrdenCompra> ordenes = aprovisionamientoBLL.ConsultarHistorialCompras(null)
                .Where(o => o.Estado == EstadoOrdenCompra.Emitida || o.Estado == EstadoOrdenCompra.RecibidaParcial)
                .ToList();
            cmbOrdenRecepcion.DataSource = null;
            cmbOrdenRecepcion.DisplayMember = "Descripcion";
            cmbOrdenRecepcion.ValueMember = "IdOrdenCompra";
            cmbOrdenRecepcion.DataSource = ordenes.Select(o => new OrdenRecepcionVista(o)).ToList();
            CambiarOrdenRecepcion(null, EventArgs.Empty);
        }

        private void CambiarOrdenRecepcion(object sender, EventArgs e)
        {
            OrdenRecepcionVista vista = cmbOrdenRecepcion.SelectedItem as OrdenRecepcionVista;
            List<Producto> productos = new List<Producto>();
            if (vista != null)
            {
                foreach (DetalleOrdenCompra detalle in vista.Orden.Detalles.Where(d => d.CantidadRecibida < d.Cantidad))
                {
                    Producto producto = productoBLL.Obtener(detalle.IdProducto);
                    if (producto != null) productos.Add(producto);
                }
            }
            Enlazar(cmbRecepcionProducto, productos, "Nombre", "IdProducto");
        }

        private void GuardarProveedor(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                proveedorBLL.Guardar(new Proveedor
                {
                    RazonSocial = txtProveedorRazon.Text.Trim(),
                    Cuit = txtProveedorCuit.Text.Trim(),
                    Contacto = txtProveedorContacto.Text.Trim(),
                    AntiguedadMercado = Convert.ToInt32(nudProveedorAntiguedad.Value),
                    IndicePrestigio = nudProveedorPrestigio.Value
                });
                CargarCatalogos();
            }, "Proveedor guardado correctamente.");
        }

        private void GuardarProducto(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                productoBLL.Guardar(new Producto
                {
                    Codigo = txtProductoCodigo.Text.Trim(),
                    Nombre = txtProductoNombre.Text.Trim(),
                    Marca = txtProductoMarca.Text.Trim(),
                    FamiliaOlfativa = txtProductoFamilia.Text.Trim(),
                    PrecioVenta = nudProductoPrecio.Value,
                    StockMinimo = Convert.ToInt32(nudProductoStockMinimo.Value)
                });
                CargarCatalogos();
            }, "Perfume guardado correctamente.");
        }

        private void RegistrarCotizacion(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                CotizacionProveedor cotizacion = new CotizacionProveedor
                {
                    IdProveedor = ValorSeleccionado(cmbCotizacionProveedor),
                    FechaVigencia = dtpCotizacionVigencia.Value.Date,
                    Condiciones = txtCotizacionCondiciones.Text.Trim()
                };
                cotizacion.Detalles.Add(new DetalleCotizacionProveedor
                {
                    IdProducto = ValorSeleccionado(cmbCotizacionProducto),
                    PrecioUnitario = nudCotizacionPrecio.Value,
                    PlazoEntregaDias = Convert.ToInt32(nudCotizacionPlazo.Value)
                });
                aprovisionamientoBLL.RegistrarCotizacion(cotizacion);
            }, "Cotización registrada correctamente.");
        }

        private void CrearOrden(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                OrdenCompra orden = new OrdenCompra { IdProveedor = ValorSeleccionado(cmbOrdenProveedor) };
                aprovisionamientoBLL.CrearOrdenPreliminar(orden, new List<DetalleOrdenCompra>
                {
                    new DetalleOrdenCompra
                    {
                        IdProducto = ValorSeleccionado(cmbOrdenProducto),
                        Cantidad = Convert.ToInt32(nudOrdenCantidad.Value),
                        PrecioUnitario = nudOrdenPrecio.Value
                    }
                });
                CargarOrdenes();
            }, "Borrador de orden creado.");
        }

        private void EnviarRevision(object sender, EventArgs e)
        {
            OrdenCompraVista orden = Seleccion(dgvOrdenes);
            if (orden == null) return;
            Ejecutar(delegate { aprovisionamientoBLL.EnviarOrdenARevisionContable(orden.IdOrdenCompra); CargarOrdenes(); }, "Orden enviada a Contabilidad.");
        }

        private void EmitirOrden(object sender, EventArgs e)
        {
            OrdenCompraVista orden = Seleccion(dgvOrdenes);
            if (orden == null) return;
            Ejecutar(delegate { aprovisionamientoBLL.EmitirOrdenAprobada(orden.IdOrdenCompra); CargarOrdenes(); CargarOrdenesRecepcion(); }, "Orden emitida al proveedor.");
        }

        private void AprobarOrden(object sender, EventArgs e)
        {
            OrdenCompraVista orden = Seleccion(dgvPendientes);
            if (orden == null) return;
            Ejecutar(delegate { aprovisionamientoBLL.AprobarOrdenCompra(orden.IdOrdenCompra, usuario.Id); CargarPendientes(); }, "Orden aprobada por Contabilidad.");
        }

        private void RechazarOrden(object sender, EventArgs e)
        {
            OrdenCompraVista orden = Seleccion(dgvPendientes);
            if (orden == null) return;
            Ejecutar(delegate { aprovisionamientoBLL.RechazarOrdenCompra(orden.IdOrdenCompra, txtMotivoRechazo.Text); CargarPendientes(); }, "Orden rechazada por Contabilidad.");
        }

        private void RegistrarRecepcion(object sender, EventArgs e)
        {
            OrdenRecepcionVista orden = cmbOrdenRecepcion.SelectedItem as OrdenRecepcionVista;
            if (orden == null) { MessageBox.Show("Seleccione una orden emitida."); return; }
            Ejecutar(delegate
            {
                Lote lote = new Lote
                {
                    IdProducto = ValorSeleccionado(cmbRecepcionProducto),
                    CodigoLote = txtLoteCodigo.Text.Trim(),
                    Origen = txtLoteOrigen.Text.Trim(),
                    CantidadIngresada = Convert.ToInt32(nudLoteCantidad.Value),
                    FechaVencimiento = dtpLoteVencimiento.Checked ? (DateTime?)dtpLoteVencimiento.Value.Date : null
                };
                aprovisionamientoBLL.RecepcionarMercaderia(orden.IdOrdenCompra, new List<Lote> { lote });
                CargarOrdenesRecepcion();
                txtLoteCodigo.Clear();
            }, "Recepción registrada con trazabilidad de origen.");
        }

        private void ConsultarInventario(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                int idProducto = ValorSeleccionado(cmbInventarioProducto);
                lblStock.Text = "Stock disponible: " + inventarioBLL.VerificarStock(idProducto);
                dgvTrazabilidad.DataSource = inventarioBLL.ObtenerTrazabilidad(idProducto);
                if (dgvTrazabilidad.Columns.Contains("CertificadoOrigen"))
                    dgvTrazabilidad.Columns["CertificadoOrigen"].Visible = false;
            }, null);
        }

        private static int ValorSeleccionado(ComboBox combo)
        {
            if (combo.SelectedValue == null) throw new InvalidOperationException("Debe seleccionar un valor de la lista.");
            return Convert.ToInt32(combo.SelectedValue);
        }

        private static OrdenCompraVista Seleccion(DataGridView grilla)
        {
            return grilla.CurrentRow == null ? null : grilla.CurrentRow.DataBoundItem as OrdenCompraVista;
        }

        private void Ejecutar(Action accion, string mensaje)
        {
            try
            {
                UltimoError = null;
                UltimoMensaje = null;
                accion();
                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    Informar(mensaje);
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void MostrarError(Exception error)
        {
            UltimoError = error.Message;
            if (!ModoPrueba)
                MessageBox.Show(error.Message, "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Informar(string mensaje)
        {
            UltimoMensaje = mensaje;
            if (!ModoPrueba) MessageBox.Show(mensaje, "Perfumería", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static FlowLayoutPanel FlujoVertical()
        {
            return new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(12) };
        }

        private static FlowLayoutPanel FlujoHorizontal()
        {
            return new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(8) };
        }

        private static GroupBox Grupo(string texto, int ancho, int alto)
        {
            return new GroupBox { Text = texto, Width = ancho, Height = alto, Padding = new Padding(8) };
        }

        private static Label Titulo(string texto)
        {
            return new Label { Text = texto, Font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold), AutoSize = true, Margin = new Padding(3, 3, 3, 10) };
        }

        private static Button Boton(string texto, string nombre, EventHandler evento)
        {
            Button boton = new Button { Text = texto, Name = nombre, AutoSize = true, Height = 32, Margin = new Padding(8, 24, 3, 3) };
            boton.Click += evento;
            return boton;
        }

        private static void AgregarCampo(FlowLayoutPanel panel, string etiqueta, Control control, int ancho)
        {
            FlowLayoutPanel bloque = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Margin = new Padding(3) };
            bloque.Controls.Add(new Label { Text = etiqueta, AutoSize = true });
            control.Width = ancho;
            bloque.Controls.Add(control);
            panel.Controls.Add(bloque);
        }

        private sealed class OrdenCompraVista
        {
            public int IdOrdenCompra { get; set; }
            public string Proveedor { get; set; }
            public string Productos { get; set; }
            public DateTime Fecha { get; set; }
            public decimal Total { get; set; }
            public string Estado { get; set; }
            public string CreadaPor { get; set; }
            public int Unidades { get; set; }

            public static OrdenCompraVista Crear(OrdenCompra orden, IDictionary<int, Proveedor> proveedores, IDictionary<int, Producto> productos)
            {
                Proveedor proveedor;
                proveedores.TryGetValue(orden.IdProveedor, out proveedor);
                return new OrdenCompraVista
                {
                    IdOrdenCompra = orden.IdOrdenCompra,
                    Proveedor = proveedor != null ? proveedor.RazonSocial : "Proveedor no disponible",
                    Productos = string.Join(Environment.NewLine, orden.Detalles.Select(d =>
                    {
                        Producto producto;
                        productos.TryGetValue(d.IdProducto, out producto);
                        string nombre = producto != null ? producto.Nombre : "Producto no disponible";
                        return nombre + " × " + d.Cantidad;
                    })),
                    Fecha = orden.FechaEmision,
                    Total = orden.Total,
                    Estado = orden.Estado.ToString(),
                    CreadaPor = orden.CreadaPor,
                    Unidades = orden.Detalles.Sum(d => d.Cantidad)
                };
            }
        }

        private sealed class OrdenRecepcionVista
        {
            public OrdenCompra Orden { get; private set; }
            public int IdOrdenCompra { get { return Orden.IdOrdenCompra; } }
            public string Descripcion { get { return "Orden " + Orden.IdOrdenCompra + " - " + Orden.Estado; } }

            public OrdenRecepcionVista(OrdenCompra orden)
            {
                Orden = orden;
            }
        }
    }
}
