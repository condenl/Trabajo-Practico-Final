using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace UI.UserControls
{
    public class UC_Ventas : UserControl, IVistaConCambiosPendientes
    {
        public bool TieneCambiosPendientes { get { return grilla.Rows.Count > 0; } }
        public bool ModoPrueba { get; set; }
        public string UltimoMensaje { get; private set; }
        public string UltimoError { get; private set; }
        private readonly VentaBLL ventaBLL = new VentaBLL();
        private readonly ProductoBLL productoBLL = new ProductoBLL();
        private readonly InventarioBLL inventarioBLL = new InventarioBLL();
        private Cliente clienteActual;
        private Venta cotizacion;
        private readonly TextBox txtEmail = new TextBox { Name = "txtEmail", Width = 245 };
        private readonly ComboBox cmbProducto = new ComboBox { Name = "cmbProducto", DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly NumericUpDown nudCantidad = new NumericUpDown { Name = "nudCantidad", Minimum = 1, Maximum = 999, Value = 1, Width = 75 };
        private readonly DataGridView grilla = new DataGridView { Name = "dgvVenta", Dock = DockStyle.Fill, ReadOnly = true,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize };
        private readonly Label lblCliente = Etiqueta("lblCliente", "Venta ocasional · sin cliente seleccionado");
        private readonly Label lblStock = Etiqueta("lblStockVenta", "Stock disponible: —");
        private readonly Label resumen = Etiqueta("lblTotalesVenta", "");
        private readonly Label estado = Etiqueta("lblEstadoVenta", "");
        private Button confirmar;

        public UC_Ventas()
        {
            Dock = DockStyle.Fill; BindingContext = new BindingContext();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(12), AutoScroll = true };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) layout.RowStyles.Add(new RowStyle(i == 4 ? SizeType.Percent : SizeType.AutoSize, i == 4 ? 100 : 0));
            layout.Controls.Add(new Label { Text = "Venta", AutoSize = true, Font = new Font(Font.FontFamily, 16, FontStyle.Bold), Margin = new Padding(3, 3, 3, 12) }, 0, 0);
            var clientes = Barra();
            clientes.Controls.Add(Etiqueta("", "Correo del cliente")); clientes.Controls.Add(txtEmail);
            clientes.Controls.Add(Boton("Buscar cliente", "btnBuscarCliente", BuscarCliente));
            clientes.Controls.Add(Boton("Nuevo cliente...", "btnNuevoCliente", NuevoCliente));
            clientes.Controls.Add(Boton("Venta ocasional", "btnQuitarCliente", delegate { QuitarCliente(); }));
            layout.Controls.Add(clientes, 0, 1); layout.Controls.Add(lblCliente, 0, 2);
            var productos = Barra();
            productos.Controls.Add(Etiqueta("", "Perfume")); productos.Controls.Add(cmbProducto);
            productos.Controls.Add(Etiqueta("", "Cantidad")); productos.Controls.Add(nudCantidad);
            productos.Controls.Add(Boton("Agregar producto", "btnAgregarProducto", AgregarProducto));
            productos.Controls.Add(Boton("Quitar producto", "btnQuitarProducto", delegate {
                Ejecutar(delegate { if (grilla.CurrentRow != null) { grilla.Rows.Remove(grilla.CurrentRow); ActualizarTotales(); ActualizarStock(); } });
            }));
            productos.Controls.Add(Boton("Actualizar", "btnActualizarVenta", delegate { Ejecutar(delegate { CargarProductos(); ActualizarTotales(); }); }));
            productos.Controls.Add(lblStock); layout.Controls.Add(productos, 0, 3);
            grilla.Columns.Add("IdProducto", "Id"); grilla.Columns[0].Visible = false;
            grilla.Columns.Add("Producto", "Producto"); grilla.Columns.Add("Cantidad", "Cantidad");
            grilla.Columns.Add("Precio", "Precio unitario"); grilla.Columns.Add("Importe", "Subtotal");
            grilla.Columns["Producto"].FillWeight = 220;
            foreach (string c in new[] { "Precio", "Importe" }) { grilla.Columns[c].DefaultCellStyle.Format = "C2"; grilla.Columns[c].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; }
            layout.Controls.Add(grilla, 0, 4);
            var pie = Barra(); resumen.Font = new Font(Font, FontStyle.Bold); pie.Controls.Add(resumen);
            confirmar = Boton("Confirmar venta", "btnConfirmarVenta", ConfirmarVenta); pie.Controls.Add(confirmar);
            layout.Controls.Add(pie, 0, 5); layout.Controls.Add(estado, 0, 6); Controls.Add(layout);
            txtEmail.TextChanged += delegate {
                if (clienteActual != null && !string.Equals(txtEmail.Text.Trim(), clienteActual.Email, StringComparison.OrdinalIgnoreCase))
                { clienteActual = null; MostrarCliente(); ActualizarTotales(); }
            };
            txtEmail.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BuscarCliente(s, EventArgs.Empty); } };
            cmbProducto.SelectedIndexChanged += delegate { Ejecutar(ActualizarStock); };
            Ejecutar(CargarProductos); ActualizarTotales();
        }
        private static Label Etiqueta(string nombre, string texto) { return new Label { Name = nombre, Text = texto, AutoSize = true, Margin = new Padding(3, 7, 10, 7) }; }
        private static FlowLayoutPanel Barra() { return new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 3, 0, 3) }; }
        private static Button Boton(string texto, string nombre, EventHandler accion) { var b = new Button { Text = texto, Name = nombre, AutoSize = true, Padding = new Padding(5) }; b.Click += accion; return b; }
        private void CargarProductos()
        {
            var anterior = cmbProducto.SelectedItem as Producto;
            cmbProducto.DisplayMember = "Nombre"; cmbProducto.ValueMember = "IdProducto"; cmbProducto.DataSource = productoBLL.Listar(true);
            if (anterior != null && cmbProducto.Items.Cast<Producto>().Any(p => p.IdProducto == anterior.IdProducto)) cmbProducto.SelectedValue = anterior.IdProducto;
            ActualizarStock();
        }
        private void ActualizarStock()
        {
            var p = cmbProducto.SelectedItem as Producto;
            if (p == null) { lblStock.Text = "Sin perfumes disponibles"; return; }
            int reservado = grilla.Rows.Cast<DataGridViewRow>().Where(r => (int)r.Cells["IdProducto"].Value == p.IdProducto).Sum(r => (int)r.Cells["Cantidad"].Value);
            lblStock.Text = "Stock vendible: " + inventarioBLL.ObtenerLotesDisponibles(p.IdProducto).Sum(l => l.CantidadDisponible) + " u. · En esta venta: " + reservado + " u.";
        }
        private void BuscarCliente(object sender, EventArgs e)
        {
            Ejecutar(delegate {
                clienteActual = ventaBLL.BuscarClientePorEmail(txtEmail.Text);
                MostrarCliente(); ActualizarTotales();
                if (clienteActual == null) Notificar("No se encontró el cliente. Puede registrarlo o continuar como venta ocasional.", false);
            });
        }
        private void NuevoCliente(object sender, EventArgs e)
        {
            Ejecutar(delegate {
                using (var alta = new FormAltaCliente(txtEmail.Text))
                    if (alta.ShowDialog(this) == DialogResult.OK) {
                        clienteActual = null; txtEmail.Text = alta.ClienteCreado.Email; clienteActual = alta.ClienteCreado;
                        MostrarCliente(); ActualizarTotales(); Notificar("Cliente registrado y seleccionado para esta venta.", false);
                    }
            });
        }
        private void MostrarCliente() { lblCliente.Text = clienteActual == null ? "Venta ocasional · sin cliente seleccionado" : "Cliente: " + clienteActual.NombreCompleto + " · " + clienteActual.Email; }
        private void QuitarCliente() { clienteActual = null; txtEmail.Clear(); MostrarCliente(); ActualizarTotales(); }
        private void AgregarProducto(object sender, EventArgs e)
        {
            Ejecutar(delegate {
                var p = cmbProducto.SelectedItem as Producto; if (p == null) return;
                var fila = grilla.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => (int)r.Cells["IdProducto"].Value == p.IdProducto);
                int cantidad = (fila == null ? 0 : (int)fila.Cells["Cantidad"].Value) + (int)nudCantidad.Value;
                if (cantidad > inventarioBLL.ObtenerLotesDisponibles(p.IdProducto).Sum(l => l.CantidadDisponible))
                    throw new InvalidOperationException("No hay stock suficiente de " + p.Nombre + " para agregar esa cantidad.");
                if (fila == null) grilla.Rows.Add(p.IdProducto, p.Nombre, cantidad, p.PrecioVenta, cantidad * p.PrecioVenta);
                else fila.Cells["Cantidad"].Value = cantidad;
                ActualizarTotales(); ActualizarStock();
            });
        }
        private Venta CrearVenta()
        {
            var venta = new Venta { IdCliente = clienteActual == null ? (int?)null : clienteActual.IdCliente };
            foreach (DataGridViewRow fila in grilla.Rows) venta.Detalles.Add(new DetalleVenta { IdProducto = (int)fila.Cells["IdProducto"].Value, Cantidad = (int)fila.Cells["Cantidad"].Value });
            return venta;
        }
        private void ActualizarTotales()
        {
            cotizacion = null; confirmar.Enabled = false;
            if (grilla.Rows.Count == 0) {
                resumen.Text = "Subtotal: $ 0 · Descuento: $ 0 · Total: $ 0";
                estado.Text = "Agregue productos para revisar el total antes de confirmar."; estado.ForeColor = TemaAplicacion.Texto; UltimoError = null; return;
            }
            try {
                var venta = CrearVenta(); ventaBLL.CotizarVenta(venta);
                foreach (DataGridViewRow fila in grilla.Rows) {
                    decimal precio = venta.Detalles.First(d => d.IdProducto == (int)fila.Cells["IdProducto"].Value).PrecioUnitario;
                    fila.Cells["Precio"].Value = precio; fila.Cells["Importe"].Value = precio * (int)fila.Cells["Cantidad"].Value;
                }
                cotizacion = venta; confirmar.Enabled = true;
                resumen.Text = "Subtotal: " + venta.Subtotal.ToString("C2") + " · Descuento: " + venta.Descuento.ToString("C2") + " · Total: " + venta.Total.ToString("C2");
                estado.Text = "Importes calculados con precios actuales y membresía activa, si corresponde."; estado.ForeColor = TemaAplicacion.Texto;
                UltimoError = null;
            } catch (Exception ex) { resumen.Text = "Total pendiente de revisión"; Notificar(ex.Message, true); }
        }
        private void ConfirmarVenta(object sender, EventArgs e)
        {
            Ejecutar(delegate {
                var anterior = cotizacion; ActualizarTotales(); if (cotizacion == null) return;
                if (anterior == null || anterior.Total != cotizacion.Total || anterior.Subtotal != cotizacion.Subtotal || anterior.Descuento != cotizacion.Descuento) {
                    Notificar("Los importes cambiaron. Revise el total actualizado y vuelva a confirmar.", false); return;
                }
                var venta = CrearVenta(); ventaBLL.RegistrarVenta(venta, cotizacion.Subtotal, cotizacion.Descuento);
                grilla.Rows.Clear(); QuitarCliente();
                Notificar("Venta #" + venta.IdVenta + " registrada. Total: " + venta.Total.ToString("C2") + ". Lista para una nueva venta ocasional.", false);
                try { CargarProductos(); } catch (Exception ex) { Notificar("Venta #" + venta.IdVenta + " registrada. No se pudo refrescar el catálogo: " + ex.Message, true); }
            });
        }
        private void Ejecutar(Action accion) { try { accion(); } catch (Exception ex) { Notificar(ex.Message, true); } }
        private void Notificar(string mensaje, bool error) {
            UltimoError = error ? mensaje : null; if (!error) UltimoMensaje = mensaje;
            estado.Text = mensaje; estado.ForeColor = TemaAplicacion.ColorMensaje(error);
        }
    }
}
