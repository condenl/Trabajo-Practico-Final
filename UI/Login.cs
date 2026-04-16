using System;
using System.Windows.Forms;
using BE;
using BLL;
using BE.Interfaces;
using Servicios;
using UI.UserControls;
using static System.Collections.Specialized.BitVector32;

namespace UI
{
    public partial class Login : Form
    {
        private readonly IVisitante visitanteBLL;

        public Login()
        {
            InitializeComponent();
            visitanteBLL = new UsuarioBLL();
        }

        private void Principal_Load(object sender, EventArgs e)
        {
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            if (txtPassword != null)
                txtPassword.UseSystemPasswordChar = true;

            panel1.Controls.Clear();
        }

        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Text;

                if (!ValidarEntradas(email, password))
                {
                    MessageBox.Show("Por favor, complete todos los campos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ProcesarLogin(email, password);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Session.Instance.Login(usuarioLogueado);
                NavegarAlSistema(usuarioLogueado);
            }
            else
            {
                MessageBox.Show("Email o contraseña incorrectos.", "Credenciales Inválidas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavegarAlSistema(Usuario usuario)
        {

            MessageBox.Show($"Bienvenido: {usuario.Nombre} ({usuario.RolAsignado})", "Acceso Concedido", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FormAdmin formAdmin = new FormAdmin(usuario);

            AbrirFormularioPrincipal(formAdmin);
        }

        private void AbrirFormularioPrincipal(Form formularioDestino)
        {
            this.Hide();

            formularioDestino.FormClosed += (s, args) =>
            {
                CerrarSesionYRestaurarLogin();
            };

            formularioDestino.Show();
        }

        private void CerrarSesionYRestaurarLogin()
        {
            Session.Instance.Logout();

            txtEmail.Clear();
            txtPassword.Clear();
            txtPassword.UseSystemPasswordChar = true;

            txtEmail.Focus();
            this.Show();
        }

        private void MostrarRegistro()
        {
            panel1.Controls.Clear();
            UC_Registro registro = new UC_Registro();
            registro.Dock = DockStyle.Fill;
            registro.OnRegresar += Registro_OnRegresar;

            panel1.Controls.Add(registro);
            registro.BringToFront();
        }

        private void Registro_OnRegresar(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}