using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BE.Soporte;
using BLL;

namespace UI.UserControls
{
    public partial class UC_Soporte : UserControl
    {
        private SoporteBLL soporteBLL;
        public event EventHandler OnCerrar;

        public UC_Soporte(string emailSugerido = "")
        {
            InitializeComponent();
            soporteBLL = new SoporteBLL();
            txtEmail.Text = emailSugerido;
            this.Load += UC_Soporte_Load;
        }

        private void UC_Soporte_Load(object sender, EventArgs e)
        {
            try
            {
                cmbTipoProblema.DataSource = soporteBLL.ObtenerTipos();
                cmbTipoProblema.DisplayMember = "Nombre";
                cmbTipoProblema.ValueMember = "Id";
                cmbTipoProblema.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            try
            {
                SolicitudSoporte nuevaSolicitud = new SolicitudSoporte
                {
                    EmailSolicitante = txtEmail.Text.Trim(),
                    Descripcion = txtDescripcion.Text.Trim()
                };

                if (cmbTipoProblema.SelectedItem != null)
                {
                    nuevaSolicitud.TipoProblema = (TipoProblema)cmbTipoProblema.SelectedItem;
                }

                soporteBLL.CrearSolicitud(nuevaSolicitud);

                MessageBox.Show("Tu solicitud fue registrada con éxito. Un administrador revisará tu caso pronto.", "Soporte Técnico", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnCerrar?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            OnCerrar?.Invoke(this, EventArgs.Empty);
        }
    }
}