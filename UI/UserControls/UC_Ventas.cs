using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace UI.UserControls
{
    public class UC_Ventas : UserControl
    {
        public bool ModoPrueba { get; set; }
        public string UltimoMensaje { get; private set; }
        public string UltimoError { get; private set; }

        private readonly VentaBLL ventaBLL = new VentaBLL();
        private readonly ProductoBLL productoBLL = new ProductoBLL();
        private Cliente clienteActual;

        private readonly TextBox txtEmail = new TextBox { Name = "txtEmail" };
        private readonly TextBox txtNombre = new TextBox { Name = "txtNombre" };
        private readonly TextBox txtApellido = new TextBox { Name = "txtApellido" };
        private readonly TextBox txtTelefono = new TextBox { Name = "txtTelefono" };
        private readonly TextBox txtPreferencias = new TextBox { Name = "txtPreferencias" };
        private readonly CheckBox chkMarketing = new CheckBox { Name = "chkMarketing", Text = "Autoriza campañas de marketing", AutoSize = true };
        private readonly CheckBox chkMembresia = new CheckBox { Name = "chkMembresia", Text = "Crear membresía", AutoSize = true, Checked = true };
        private readonly NumericUpDown nudDescuento = new NumericUpDown { Name = "nudDescuento", Minimum = 0, Maximum = 30, Value = 10 };
        private readonly ComboBox cmbProducto = new ComboBox { Name = "cmbProducto", DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown nudCantidad = new NumericUpDown { Name = "nudCantidad", Minimum = 1, Maximum = 999, Value = 1 };
        private readonly DataGridView grilla = new DataGridView();
        private readonly Label lblCliente = new Label { AutoSize = true, Text = "Cliente ocasional" };

        public UC_Ventas()
        {
            Dock = DockStyle.Fill;
            AutoScroll = true;
            ConstruirInterfaz();
            CargarProductos();
        }

        private void ConstruirInterfaz()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                Padding = new Padding(16)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            Label titulo = new Label
            {
                Text = "Venta y fidelización de clientes",
                Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(3, 3, 3, 16)
            };
            layout.Controls.Add(titulo, 0, 0);
            layout.SetColumnSpan(titulo, 4);

            Button buscar = new Button { Text = "Buscar cliente", AutoSize = true };
            buscar.Click += BuscarCliente;
            layout.Controls.Add(new Label { Text = "Correo electrónico", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            txtEmail.Dock = DockStyle.Fill;
            layout.Controls.Add(txtEmail, 1, 1);
            layout.Controls.Add(buscar, 2, 1);
            layout.Controls.Add(lblCliente, 3, 1);

            AgregarFila(layout, 2, "Nombre", txtNombre, "Apellido", txtApellido);
            AgregarFila(layout, 3, "Teléfono", txtTelefono, "Preferencias olfativas", txtPreferencias);
            FlowLayoutPanel opcionesCliente = new FlowLayoutPanel
            {
                Name = "opcionesCliente",
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true
            };
            chkMarketing.Margin = new Padding(3, 6, 18, 3);
            chkMembresia.Margin = new Padding(3, 6, 18, 3);
            opcionesCliente.Controls.Add(chkMarketing);
            opcionesCliente.Controls.Add(chkMembresia);
            opcionesCliente.Controls.Add(new Label { Text = "Descuento (%)", AutoSize = true, Margin = new Padding(3, 6, 3, 3) });
            opcionesCliente.Controls.Add(nudDescuento);
            layout.Controls.Add(opcionesCliente, 0, 4);
            layout.SetColumnSpan(opcionesCliente, 4);

            Button registrar = new Button { Text = "Registrar cliente", AutoSize = true };
            registrar.Click += RegistrarCliente;
            layout.Controls.Add(registrar, 1, 5);

            Button agregar = new Button { Text = "Agregar producto", AutoSize = true };
            agregar.Click += AgregarProducto;
            AgregarFila(layout, 6, "Producto", cmbProducto, "Cantidad", nudCantidad);
            layout.Controls.Add(agregar, 3, 7);

            grilla.AllowUserToAddRows = false;
            grilla.AllowUserToDeleteRows = true;
            grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grilla.Height = 220;
            grilla.Dock = DockStyle.Fill;
            grilla.Columns.Add("IdProducto", "Id");
            grilla.Columns.Add("Producto", "Producto");
            grilla.Columns.Add("Cantidad", "Cantidad");
            grilla.Columns.Add("Precio", "Precio unitario");
            grilla.Columns[0].Visible = false;
            layout.Controls.Add(grilla, 0, 8);
            layout.SetColumnSpan(grilla, 4);

            Button confirmar = new Button { Text = "Confirmar venta", AutoSize = true };
            confirmar.Click += ConfirmarVenta;
            layout.Controls.Add(confirmar, 3, 9);
            Controls.Add(layout);
        }

        private static void AgregarFila(TableLayoutPanel layout, int fila, string etiqueta1, Control control1, string etiqueta2, Control control2)
        {
            layout.Controls.Add(new Label { Text = etiqueta1, AutoSize = true, Anchor = AnchorStyles.Left }, 0, fila);
            control1.Dock = DockStyle.Fill;
            layout.Controls.Add(control1, 1, fila);
            layout.Controls.Add(new Label { Text = etiqueta2, AutoSize = true, Anchor = AnchorStyles.Left }, 2, fila);
            control2.Dock = DockStyle.Fill;
            layout.Controls.Add(control2, 3, fila);
        }

        private void CargarProductos()
        {
            cmbProducto.DataSource = productoBLL.Listar(true);
            cmbProducto.DisplayMember = "Nombre";
            cmbProducto.ValueMember = "IdProducto";
        }

        private void BuscarCliente(object sender, EventArgs e)
        {
            clienteActual = ventaBLL.BuscarClientePorEmail(txtEmail.Text);
            lblCliente.Text = clienteActual == null ? "Cliente no registrado" : clienteActual.NombreCompleto;
        }

        private void RegistrarCliente(object sender, EventArgs e)
        {
            try
            {
                Cliente cliente = new Cliente
                {
                    Nombre = txtNombre.Text,
                    Apellido = txtApellido.Text,
                    Email = txtEmail.Text,
                    Telefono = txtTelefono.Text,
                    PreferenciasOlfativas = txtPreferencias.Text,
                    AutorizaMarketing = chkMarketing.Checked
                };
                ventaBLL.RegistrarCliente(cliente, chkMembresia.Checked, nudDescuento.Value);
                clienteActual = cliente;
                lblCliente.Text = cliente.NombreCompleto;
                Notificar("Cliente registrado correctamente.", false);
            }
            catch (Exception error)
            {
                Notificar(error.Message, true);
            }
        }

        private void AgregarProducto(object sender, EventArgs e)
        {
            Producto producto = cmbProducto.SelectedItem as Producto;
            if (producto == null) return;

            DataGridViewRow existente = grilla.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(r => Convert.ToInt32(r.Cells["IdProducto"].Value) == producto.IdProducto);
            if (existente == null)
                grilla.Rows.Add(producto.IdProducto, producto.Nombre, Convert.ToInt32(nudCantidad.Value), producto.PrecioVenta);
            else
                existente.Cells["Cantidad"].Value = Convert.ToInt32(existente.Cells["Cantidad"].Value) + Convert.ToInt32(nudCantidad.Value);
        }

        private void ConfirmarVenta(object sender, EventArgs e)
        {
            try
            {
                Venta venta = new Venta { IdCliente = clienteActual != null ? (int?)clienteActual.IdCliente : null };
                foreach (DataGridViewRow fila in grilla.Rows)
                {
                    venta.Detalles.Add(new DetalleVenta
                    {
                        IdProducto = Convert.ToInt32(fila.Cells["IdProducto"].Value),
                        Cantidad = Convert.ToInt32(fila.Cells["Cantidad"].Value)
                    });
                }

                ventaBLL.RegistrarVenta(venta);
                Notificar("Venta registrada. Total: " + venta.Total.ToString("C2") + " (descuento: " + venta.Descuento.ToString("C2") + ")", false);
                grilla.Rows.Clear();
            }
            catch (Exception error)
            {
                Notificar(error.Message, true);
            }
        }

        private void Notificar(string mensaje, bool error)
        {
            if (error) UltimoError = mensaje;
            else
            {
                UltimoError = null;
                UltimoMensaje = mensaje;
            }

            if (!ModoPrueba)
                MessageBox.Show(mensaje, error ? "No se pudo completar la operación" : "Perfumería", MessageBoxButtons.OK,
                    error ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
    }
}
