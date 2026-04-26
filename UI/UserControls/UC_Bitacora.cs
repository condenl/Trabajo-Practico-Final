using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;

namespace UI.UserControls
{
    public partial class UC_Bitacora : UserControl
    {
        public UC_Bitacora()
        {
            InitializeComponent();
        }

        private void UC_Bitacora_Load(object sender, EventArgs e)
        {
            CargarComboActividades();
            chkFechas.Checked = false;
            CambiarEstadoFechas(false);
            EjecutarBusqueda();
        }

        private void CargarComboActividades()
        {
            cmbActividad.Items.Clear();
            cmbActividad.Items.Add("Todas"); 
            cmbActividad.Items.Add("LOGIN");
            cmbActividad.Items.Add("LOGIN_FALLIDO");
            cmbActividad.Items.Add("ALTA_USUARIO");
            cmbActividad.Items.Add("MODIFICACION");
            cmbActividad.Items.Add("BLOQUEO");
            cmbActividad.Items.Add("SEGURIDAD");

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
                string actividad = cmbActividad.SelectedIndex > 0 ? cmbActividad.SelectedItem.ToString() : null;

                List<EventoBitacora> resultados = BitacoraBLL.Instancia.Consultar(desde, hasta, usuario, actividad);
                dgvBitacora.DataSource = null;
                dgvBitacora.DataSource = resultados;
                ConfigurarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar la bitácora: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarGrilla()
        {
            if (dgvBitacora.Columns.Count > 0)
            {
                dgvBitacora.Columns["Id"].Visible = false;
                dgvBitacora.Columns["FechaHora"].HeaderText = "Fecha y Hora";
                dgvBitacora.Columns["UsuarioEmail"].HeaderText = "Usuario";
                dgvBitacora.Columns["InformacionAsociada"].HeaderText = "Detalle";
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