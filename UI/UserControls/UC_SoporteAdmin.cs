using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE.Soporte;
using BLL;
using Servicios;
using BE.Interfaces;
using BE;

namespace UI.UserControls
{
    public partial class UC_SoporteAdmin : UserControl, IIdiomaObserver
    {
        private SoporteBLL soporteBLL;
        private TraduccionBLL traduccionBLL;
        private SolicitudSoporte solicitudSeleccionada;

        public UC_SoporteAdmin()
        {
            InitializeComponent();
            soporteBLL = new SoporteBLL();
            traduccionBLL = new TraduccionBLL();

            this.Load += UC_SoporteAdmin_Load;
            dgvSolicitudes.CellFormatting += DgvSolicitudes_CellFormatting;
            dgvSolicitudes.CellClick += dgvSolicitudes_CellClick;
            btnGuardar.Click += btnGuardar_Click;
            this.Disposed += (s, e) => GestorIdioma.Instancia.Desuscribir(this);
        }


        private void UC_SoporteAdmin_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
            CargarCombos();
            CargarGrilla(null); 
            cmbFiltroEstado.SelectedIndexChanged += cmbFiltroEstado_SelectedIndexChanged;
        }

        public void ActualizarIdioma(Idioma idioma)
        {
            ActualizarTextos();
        }

        private void ActualizarTextos()
        {
            ActualizarTextosRecursivo(this.Controls);
        }

        private void ActualizarTextosRecursivo(Control.ControlCollection controles)
        {
            foreach (Control control in controles)
            {
                if (control.Tag != null && !string.IsNullOrWhiteSpace(control.Tag.ToString()))
                {
                    control.Text = traduccionBLL.Traducir(control.Tag.ToString());
                }
                if (control.HasChildren)
                {
                    ActualizarTextosRecursivo(control.Controls);
                }
            }
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
                string todas = traduccionBLL.Traducir("lbl_all_requests");
                estadosParaFiltro.Add(new EstadoSolicitud { Id = 0, Nombre = todas });
                estadosParaFiltro.AddRange(estadosParaEdicion);

                cmbFiltroEstado.DataSource = estadosParaFiltro;
                cmbFiltroEstado.DisplayMember = "Nombre";
                cmbFiltroEstado.ValueMember = "Id";
                cmbFiltroEstado.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_error_loading_status") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (dgvSolicitudes.Columns.Contains("Descripcion")) dgvSolicitudes.Columns["Descripcion"].Visible = false;
                if (dgvSolicitudes.Columns.Contains("ObservacionesAdmin")) dgvSolicitudes.Columns["ObservacionesAdmin"].Visible = false;
                
                if (dgvSolicitudes.Columns.Contains("EmailSolicitante")) 
                    dgvSolicitudes.Columns["EmailSolicitante"].HeaderText = traduccionBLL.Traducir("grid_user_email");
                
                if (dgvSolicitudes.Columns.Contains("FechaCreacion"))
                {
                    dgvSolicitudes.Columns["FechaCreacion"].HeaderText = traduccionBLL.Traducir("grid_date");
                    dgvSolicitudes.Columns["FechaCreacion"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }

                if (!dgvSolicitudes.Columns.Contains("ColTipoProblema"))
                {
                    dgvSolicitudes.Columns.Add(new DataGridViewTextBoxColumn { 
                        Name = "ColTipoProblema", 
                        HeaderText = traduccionBLL.Traducir("grid_problem"), 
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill 
                    });
                }
                else
                {
                    dgvSolicitudes.Columns["ColTipoProblema"].HeaderText = traduccionBLL.Traducir("grid_problem");
                }

                if (!dgvSolicitudes.Columns.Contains("ColEstado"))
                {
                    dgvSolicitudes.Columns.Add(new DataGridViewTextBoxColumn { 
                        Name = "ColEstado", 
                        HeaderText = traduccionBLL.Traducir("grid_status"), 
                        Width = 100 
                    });
                }
                else
                {
                    dgvSolicitudes.Columns["ColEstado"].HeaderText = traduccionBLL.Traducir("grid_status");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_error_loading_requests") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
                string fechaLbl = traduccionBLL.Traducir("lbl_date");
                string usuarioLbl = traduccionBLL.Traducir("lbl_user");
                string motivoLbl = traduccionBLL.Traducir("lbl_reason");
                string descLbl = traduccionBLL.Traducir("lbl_report_desc");

                txtDetalle.Text = $"{fechaLbl}: {solicitudSeleccionada.FechaCreacion:dd/MM/yyyy HH:mm}\r\n" +
                                  $"{usuarioLbl}: {solicitudSeleccionada.EmailSolicitante}\r\n" +
                                  $"{motivoLbl}: {solicitudSeleccionada.TipoProblema.Nombre}\r\n" +
                                  $"------------------------------------------------------\r\n" +
                                  $"{descLbl}:\r\n{solicitudSeleccionada.Descripcion}";

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
                MessageBox.Show(traduccionBLL.Traducir("msg_select_request_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbCambiarEstado.SelectedItem == null)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_select_status_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                solicitudSeleccionada.ObservacionesAdmin = txtObservaciones.Text.Trim();
                solicitudSeleccionada.Estado = (EstadoSolicitud)cmbCambiarEstado.SelectedItem;
                soporteBLL.ModificarSolicitudPorAdmin(solicitudSeleccionada);

                MessageBox.Show(traduccionBLL.Traducir("msg_request_updated_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                var estadoFiltro = (EstadoSolicitud)cmbFiltroEstado.SelectedItem;
                int? idFiltro = estadoFiltro.Id == 0 ? (int?)null : estadoFiltro.Id;
                CargarGrilla(idFiltro);
                LimpiarDetalle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_error_saving_changes") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
