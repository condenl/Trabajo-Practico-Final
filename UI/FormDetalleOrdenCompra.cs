using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;

namespace UI
{
    public sealed class FormDetalleOrdenCompra : Form
    {
        public FormDetalleOrdenCompra(OrdenCompra orden, string proveedor, IEnumerable<Producto> productos, string aprobador)
        {
            Text = "Detalle de orden #" + orden.IdOrdenCompra; StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font; ClientSize = new Size(900, 490); MinimumSize = new Size(680, 400);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            string estado = orden.Estado == EstadoOrdenCompra.PendienteAprobacion ? "Pendiente de aprobación" : orden.Estado == EstadoOrdenCompra.RecibidaParcial ? "Recibida parcialmente" : orden.Estado.ToString();
            layout.Controls.Add(new Label { Name = "lblCabeceraOrden", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 10),
                Text = proveedor + " · " + estado + "\nFecha: " + orden.FechaEmision.ToString("dd/MM/yyyy") + " · Creada por: " + orden.CreadaPor }, 0, 0);
            var catalogo = productos.ToDictionary(p => p.IdProducto);
            var grilla = new DataGridView { Name = "dgvDetalleOrden", Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                BindingContext = new BindingContext(), RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells };
            grilla.DataSource = orden.Detalles.Select(d => new { Producto = catalogo.ContainsKey(d.IdProducto) ? catalogo[d.IdProducto].Nombre : "Producto no disponible",
                Pedidas = d.Cantidad, Recibidas = d.CantidadRecibida, Pendientes = d.Cantidad - d.CantidadRecibida, PrecioUnitario = d.PrecioUnitario, Subtotal = d.Subtotal }).ToList();
            grilla.Columns["Producto"].FillWeight = 220; grilla.Columns["PrecioUnitario"].HeaderText = "Precio unitario";
            foreach (string columna in new[] { "PrecioUnitario", "Subtotal" }) grilla.Columns[columna].DefaultCellStyle.Format = "C2";
            grilla.DefaultCellStyle.WrapMode = DataGridViewTriState.True; layout.Controls.Add(grilla, 0, 1);
            string revision = orden.FechaAprobacion.HasValue ? "Aprobación: " + orden.FechaAprobacion.Value.ToString("dd/MM/yyyy HH:mm") + " · " + (aprobador ?? "Responsable no disponible") : "Sin aprobación contable registrada.";
            if (!string.IsNullOrWhiteSpace(orden.MotivoRechazo)) revision += "\nMotivo del rechazo: " + orden.MotivoRechazo;
            layout.Controls.Add(new Label { Name = "lblRevisionOrden", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 10), Text = "Total: " + orden.Total.ToString("C2") + "\n" + revision }, 0, 2);
            var cerrar = new Button { Name = "btnCerrarDetalleOrden", Text = "Cerrar", AutoSize = true, Padding = new Padding(5), DialogResult = DialogResult.Cancel };
            layout.Controls.Add(cerrar, 0, 3); Controls.Add(layout); CancelButton = cerrar;
            TemaAplicacion.Aplicar(this);
        }
    }
}
