using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BE;
using BE.Interfaces;
using BE.Soporte;
using BLL;
using Servicios;

namespace UI.UserControls
{
    public partial class UC_Soporte : UserControl, IIdiomaObserver
    {
        private SoporteBLL soporteBLL;
        private TraduccionBLL traduccionBLL = new TraduccionBLL();
        public event EventHandler OnCerrar;

        public UC_Soporte(string emailSugerido = "")
        {
            InitializeComponent();
            soporteBLL = new SoporteBLL();
            txtEmail.Text = emailSugerido;
            this.Load += UC_Soporte_Load;
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

        private void UC_Soporte_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
            
            try
            {
                cmbTipoProblema.DataSource = soporteBLL.ObtenerTipos();
                cmbTipoProblema.DisplayMember = "Nombre";
                cmbTipoProblema.ValueMember = "Id";
                cmbTipoProblema.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{traduccionBLL.Traducir("msg_error_connection")}: {ex.Message}");
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

                MessageBox.Show(traduccionBLL.Traducir("msg_request_registered_success"), traduccionBLL.Traducir("lbl_titulo_soporte"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnCerrar?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            OnCerrar?.Invoke(this, EventArgs.Empty);
        }
    }
}
