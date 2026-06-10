using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;
using BE.Interfaces;
using BE.Excepciones;
using Servicios;
using UI.UserControls;

namespace UI
{
    public partial class Login : Form, IIdiomaObserver
    {
        private readonly IVisitante visitanteBLL;
        private readonly IdiomaBLL idiomaBLL;
        private readonly TraduccionBLL traduccionBLL;

        public Login()
        {
            InitializeComponent();
            visitanteBLL = new UsuarioBLL();
            idiomaBLL = new IdiomaBLL();
            traduccionBLL = new TraduccionBLL();
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            if (txtPassword != null)
                txtPassword.UseSystemPasswordChar = true;

            panel1.Controls.Clear();
            panel1.Visible = false;
        }

        private void ConfigurarIdiomas()
        {
            cmbIdioma.DataSource = null;
            cmbIdioma.DataSource = idiomaBLL.Listar();
            cmbIdioma.DisplayMember = "Nombre";
            
            GestorIdioma.Instancia.Suscribir(this);

            List<Idioma> idiomas = (List<Idioma>)cmbIdioma.DataSource;
            Idioma defaultIdioma = idiomas.Find(i => i.EsDefault);
            if (defaultIdioma != null)
            {
                cmbIdioma.SelectedItem = defaultIdioma;
                GestorIdioma.Instancia.SeleccionarIdioma(defaultIdioma);
            }
        }

        public void ActualizarIdioma(Idioma idioma)
        {
            ActualizarTextos();
        }

        private void ActualizarTextos()
        {
            foreach (Control control in this.Controls)
            {
                if (control.Tag != null)
                {
                    control.Text = traduccionBLL.Traducir(control.Tag.ToString());
                }
            }
            this.Text = traduccionBLL.Traducir("login_form_title");
        }

        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Text;

                if (!ValidarEntradas(email, password))
                {
                    MessageBox.Show(traduccionBLL.Traducir("msg_complete_fields"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ProcesarLogin(email, password);
            }
            catch (UsuarioBloqueadoException ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_denied"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (UsuarioSinRolException ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_unexpected_error") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegistro_Click(object sender, EventArgs e)
        {
            MostrarRegistro();
        }

        private bool ValidarEntradas(string email, string password)
        {
            return !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password);
        }

        private void ProcesarLogin(string email, string password)
        {
            Usuario usuarioLogueado = visitanteBLL.Login(email, password);

            if (usuarioLogueado != null)
            {
                NavegarAlSistema(usuarioLogueado);
            }
            else
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_invalid_credentials"), traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavegarAlSistema(Usuario usuario)
        {
            MessageBox.Show($"{traduccionBLL.Traducir("msg_welcome")}: {usuario.Nombre}", traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            FormPrincipal formPrincipal = new FormPrincipal(usuario);
            AbrirFormularioPrincipal(formPrincipal);
        }

        private void AbrirFormularioPrincipal(Form formularioDestino)
        {
            this.Hide();
            formularioDestino.FormClosed += (s, args) => CerrarSesionYRestaurarLogin();
            formularioDestino.Show();
        }

        private void CerrarSesionYRestaurarLogin()
        {
            visitanteBLL.Logout();
            txtEmail.Clear();
            txtPassword.Clear();
            txtEmail.Focus();
            this.Show();
            ActualizarTextos();
        }

        private void MostrarRegistro()
        {
            UC_Registro registro = new UC_Registro();
            registro.Dock = DockStyle.Fill;
            registro.OnRegresar += Registro_OnRegresar;

            panel1.Controls.Clear();
            panel1.Controls.Add(registro);
            panel1.Visible = true;
            panel1.BringToFront();
        }

        private void MostrarPanelSoporte()
        {
            UC_Soporte panelSoporte = new UC_Soporte(txtEmail.Text.Trim());
            panelSoporte.Dock = DockStyle.Fill;
            panelSoporte.OnCerrar += Soporte_OnCerrar;

            panel1.Controls.Clear();
            panel1.Controls.Add(panelSoporte);
            panel1.Visible = true;
            panel1.BringToFront();
        }

        private void Soporte_OnCerrar(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel1.Visible = false;
        }

        private void Registro_OnRegresar(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel1.Visible = false;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            ConfigurarIdiomas();
        }

        private void btnSoporte_Click(object sender, EventArgs e)
        {
            MostrarPanelSoporte();
        }

        private void cmbIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIdioma.SelectedItem != null)
            {
                Idioma seleccionado = (Idioma)cmbIdioma.SelectedItem;
                GestorIdioma.Instancia.SeleccionarIdioma(seleccionado);
            }
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            GestorIdioma.Instancia.Desuscribir(this);
        }
    }
}
