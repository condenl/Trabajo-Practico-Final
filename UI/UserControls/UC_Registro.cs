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
using BLL;
using Servicios;

namespace UI.UserControls
{
    public partial class UC_Registro : UserControl, IIdiomaObserver
    {
        public event EventHandler OnRegresar;
        private IVisitante usuarioBLL;
        private TraduccionBLL traduccionBLL = new TraduccionBLL();

        public UC_Registro()
        {
            InitializeComponent();
            usuarioBLL = new UsuarioBLL();

            if (txtPassword != null)
                txtPassword.UseSystemPasswordChar = true;

            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
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

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDni.Text) ||
                    string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    string.IsNullOrWhiteSpace(txtApellido.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show(traduccionBLL.Traducir("msg_complete_fields"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!txtDni.Text.All(char.IsDigit))
                {
                    MessageBox.Show(traduccionBLL.Traducir("msg_dni_digits_only"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Usuario nuevoUsuario = new Usuario
                {
                    Dni = txtDni.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Activo = true
                };

                usuarioBLL.Registrar(nuevoUsuario, txtPassword.Text);

                MessageBox.Show(traduccionBLL.Traducir("msg_user_registered_success"), traduccionBLL.Traducir("lbl_titulo_registro"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimpiarCampos();
                OnRegresar?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            txtDni.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            OnRegresar?.Invoke(this, EventArgs.Empty);
        }
    }
}
