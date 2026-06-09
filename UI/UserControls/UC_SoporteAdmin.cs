using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE.Soporte;
using BLL;
using Servicios; 

namespace UI.UserControls
{
    public partial class UC_SoporteAdmin : UserControl
    {
        private SoporteBLL soporteBLL;
        private SolicitudSoporte solicitudSeleccionada;

        public UC_SoporteAdmin()
        {
            InitializeComponent();
            soporteBLL = new SoporteBLL();

            this.Load += UC_SoporteAdmin_Load;
            dgvSolicitudes.CellFormatting += DgvSolicitudes_CellFormatting;
            dgvSolicitudes.CellClick += dgvSolicitudes_CellClick;
            btnGuardar.Click += btnGuardar_Click;
        }

        private void UC_SoporteAdmin_Load(object sender, EventArgs e)
        {
            CargarCombos();
            CargarGrilla(null); 
            cmbFiltroEstado.SelectedIndexChanged += cmbFiltroEstado_SelectedIndexChanged;
        }

        private void CargarCombos()
        {
            try
            {
                var estadosParaEdicion = soporteBLL.ObtenerEstados();

                cmbCambiarEstado.DataSource = new List<EstadoSolicitud>(estadosParaEdicion);
                cmbCambiarEstado.DisplayMember = "Nombre";
                cmbCambiarEstado.ValueMember = "Id";
                cmbCambiarEstado.SelectedIndex = -1;

                var estadosParaFiltro = new List<EstadoSolicitud>();
                estadosParaFiltro.Add(new EstadoSolicitud { Id = 0, Nombre = "-- Todas las solicitudes --" });
                estadosParaFiltro.AddRange(estadosParaEdicion);

                cmbFiltroEstado.DataSource = estadosParaFiltro;
                cmbFiltroEstado.DisplayMember = "Nombre";
                cmbFiltroEstado.ValueMember = "Id";
                cmbFiltroEstado.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los estados: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarGrilla(int? idEstadoFiltro)
        {
            try
            {
                var listaSolicitudes = soporteBLL.ListarSolicitudes(idEstadoFiltro);

                dgvSolicitudes.DataSource = null;
                dgvSolicitudes.DataSource = listaSolicitudes;

                if (dgvSolicitudes.Columns.Contains("Id")) dgvSolicitudes.Columns["Id"].Width = 40;
                if (dgvSolicitudes.Columns.Contains("TipoProblema")) dgvSolicitudes.Columns["TipoProblema"].Visible = false;
                if (dgvSolicitudes.Columns.Contains("Estado")) dgvSolicitudes.Columns["Estado"].Visible = false;
                if (dgvSolicitudes.Columns.Contains("Descripcion")) dgvSolicitudes.Columns["Descripcion"].Visible = false; // Se ve en el txtDetalle
                if (dgvSolicitudes.Columns.Contains("ObservacionesAdmin")) dgvSolicitudes.Columns["ObservacionesAdmin"].Visible = false; // Se ve en el txtObservaciones
                if (dgvSolicitudes.Columns.Contains("EmailSolicitante")) dgvSolicitudes.Columns["EmailSolicitante"].HeaderText = "Usuario / Email";
                if (dgvSolicitudes.Columns.Contains("FechaCreacion"))
                {
                    dgvSolicitudes.Columns["FechaCreacion"].HeaderText = "Fecha";
                    dgvSolicitudes.Columns["FechaCreacion"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }

                if (!dgvSolicitudes.Columns.Contains("ColTipoProblema"))
                {
                    dgvSolicitudes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColTipoProblema", HeaderText = "Problema", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                }
                if (!dgvSolicitudes.Columns.Contains("ColEstado"))
                {
                    dgvSolicitudes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColEstado", HeaderText = "Estado", Width = 100 });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las solicitudes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSolicitudes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SolicitudSoporte solicitud = (SolicitudSoporte)dgvSolicitudes.Rows[e.RowIndex].DataBoundItem;

                if (dgvSolicitudes.Columns[e.ColumnIndex].Name == "ColTipoProblema")
                {
                    e.Value = solicitud.TipoProblema?.Nombre;
                }
                else if (dgvSolicitudes.Columns[e.ColumnIndex].Name == "ColEstado")
                {
                    e.Value = solicitud.Estado?.Nombre;
                }
            }
        }

        private void cmbFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFiltroEstado.SelectedItem != null)
            {
                var estadoSeleccionado = (EstadoSolicitud)cmbFiltroEstado.SelectedItem;
                int? idFiltro = estadoSeleccionado.Id == 0 ? (int?)null : estadoSeleccionado.Id;

                CargarGrilla(idFiltro);
                LimpiarDetalle();
            }
        }

        private void dgvSolicitudes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvSolicitudes.SelectedRows.Count > 0)
            {
                solicitudSeleccionada = (SolicitudSoporte)dgvSolicitudes.SelectedRows[0].DataBoundItem;
                txtDetalle.Text = $"FECHA: {solicitudSeleccionada.FechaCreacion:dd/MM/yyyy HH:mm}\r\n" +
                                  $"USUARIO: {solicitudSeleccionada.EmailSolicitante}\r\n" +
                                  $"MOTIVO: {solicitudSeleccionada.TipoProblema.Nombre}\r\n" +
                                  $"------------------------------------------------------\r\n" +
                                  $"DESCRIPCIÓN DEL REPORTE:\r\n{solicitudSeleccionada.Descripcion}";

                txtObservaciones.Text = solicitudSeleccionada.ObservacionesAdmin;

                if (solicitudSeleccionada.Estado != null && solicitudSeleccionada.Estado.Id > 0)
                {
                    cmbCambiarEstado.SelectedValue = solicitudSeleccionada.Estado.Id;
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (solicitudSeleccionada == null)
            {
                MessageBox.Show("Por favor, seleccione una solicitud de la lista para actualizarla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbCambiarEstado.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un estado para la solicitud.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                solicitudSeleccionada.ObservacionesAdmin = txtObservaciones.Text.Trim();
                solicitudSeleccionada.Estado = (EstadoSolicitud)cmbCambiarEstado.SelectedItem;
                soporteBLL.ModificarSolicitudPorAdmin(solicitudSeleccionada);

                MessageBox.Show("La solicitud fue actualizada y registrada exitosamente.", "Soporte Actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var estadoFiltro = (EstadoSolicitud)cmbFiltroEstado.SelectedItem;
                int? idFiltro = estadoFiltro.Id == 0 ? (int?)null : estadoFiltro.Id;
                CargarGrilla(idFiltro);
                LimpiarDetalle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar guardar los cambios: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarDetalle()
        {
            solicitudSeleccionada = null;
            txtDetalle.Clear();
            txtObservaciones.Clear();
            cmbCambiarEstado.SelectedIndex = -1;
        }
    }
}