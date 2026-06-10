using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BE.Interfaces;
using BLL;
using Servicios;

namespace UI.UserControls
{
    public partial class UC_Bitacora : UserControl, IIdiomaObserver
    {
        private TraduccionBLL traduccionBLL = new TraduccionBLL();
        
        // Activity codes
        private readonly string[] ACTIVIDADES = { "LOGIN", "LOGIN_FALLIDO", "ALTA_USUARIO", "MODIFICACION", "BLOQUEO", "SEGURIDAD" };

        public UC_Bitacora()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GestorIdioma.Instancia.Desuscribir(this);
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void UC_Bitacora_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
            CargarComboActividades();
            chkFechas.Checked = false;
            CambiarEstadoFechas(false);
            EjecutarBusqueda();
        }

        public void ActualizarIdioma(Idioma idioma)
        {
            ActualizarTextos();
            CargarComboActividades();
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

        private void CargarComboActividades()
        {
            int previousIndex = cmbActividad.SelectedIndex;
            
            cmbActividad.Items.Clear();
            cmbActividad.Items.Add(traduccionBLL.Traducir("lbl_all_activities")); // Index 0

            foreach (var act in ACTIVIDADES)
            {
                cmbActividad.Items.Add(traduccionBLL.Traducir("activity_" + act));
            }

            if (previousIndex >= 0 && previousIndex < cmbActividad.Items.Count)
                cmbActividad.SelectedIndex = previousIndex;
            else
                cmbActividad.SelectedIndex = 0;

            cmbActividad.DropDownStyle = ComboBoxStyle.DropDownList; 
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            EjecutarBusqueda();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            chkFechas.Checked = false;
            txtUsuario.Clear();
            cmbActividad.SelectedIndex = 0;
            EjecutarBusqueda();
        }

        private void chkFechas_CheckedChanged(object sender, EventArgs e)
        {
            CambiarEstadoFechas(chkFechas.Checked);
        }

        private void CambiarEstadoFechas(bool estado)
        {
            dtpDesde.Enabled = estado;
            dtpHasta.Enabled = estado;
        }

        private void EjecutarBusqueda()
        {
            try
            {
                DateTime? desde = chkFechas.Checked ? dtpDesde.Value.Date : (DateTime?)null;
                DateTime? hasta = chkFechas.Checked ? dtpHasta.Value.Date : (DateTime?)null;
                string usuario = string.IsNullOrWhiteSpace(txtUsuario.Text) ? null : txtUsuario.Text.Trim();
                
                // Use the technical key from the array based on index
                string actividad = null;
                if (cmbActividad.SelectedIndex > 0)
                {
                    actividad = ACTIVIDADES[cmbActividad.SelectedIndex - 1];
                }

                List<EventoBitacora> resultados = BitacoraBLL.Instancia.Consultar(desde, hasta, usuario, actividad);
                dgvBitacora.DataSource = null;
                dgvBitacora.DataSource = resultados;
                ConfigurarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{traduccionBLL.Traducir("msg_error_consulting_log")}: {ex.Message}", traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarGrilla()
        {
            if (dgvBitacora.Columns.Count > 0)
            {
                dgvBitacora.Columns["Id"].Visible = false;
                dgvBitacora.Columns["FechaHora"].HeaderText = traduccionBLL.Traducir("grid_date_time");
                dgvBitacora.Columns["UsuarioEmail"].HeaderText = traduccionBLL.Traducir("grid_user");
                dgvBitacora.Columns["InformacionAsociada"].HeaderText = traduccionBLL.Traducir("grid_detail");
                dgvBitacora.Columns["InformacionAsociada"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvBitacora.Columns["FechaHora"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            }
        }

        private void dtpDesde_ValueChanged(object sender, EventArgs e)
        {
            if (dtpDesde.Value > dtpHasta.Value)
            {
                dtpHasta.Value = dtpDesde.Value;
            }
        }

        private void dtpHasta_ValueChanged(object sender, EventArgs e)
        {
            if (dtpHasta.Value < dtpDesde.Value)
            {
                dtpDesde.Value = dtpHasta.Value;
            }
        }
    }
}
