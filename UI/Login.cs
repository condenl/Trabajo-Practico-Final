using System;
using System.Windows.Forms;
using BE;
using BLL;
using BE.Interfaces;
using BE.Excepciones;
using Servicios;
using UI.UserControls;

namespace UI
{
    public partial class Login : Form
    {
        private readonly IVisitante visitanteBLL;

        public Login()
        {
            InitializeComponent();
            visitanteBLL = new UsuarioBLL();
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            if (txtPassword != null)
                txtPassword.UseSystemPasswordChar = true;

            panel1.Controls.Clear();
            panel1.Visible = false;
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
            catch (UsuarioBloqueadoException ex)
            {
                MessageBox.Show(ex.Message, "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (UsuarioSinRolException ex)
            {
                // Atajamos a los usuarios huérfanos y los mandamos a soporte
                MessageBox.Show(ex.Message, "Falta de Permisos", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                NavegarAlSistema(usuarioLogueado);
            }
            else
            {
                MessageBox.Show("Email o contraseña incorrectos.", "Credenciales Inválidas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavegarAlSistema(Usuario usuario)
        {
            MessageBox.Show($"Bienvenido: {usuario.Nombre}", "Acceso Concedido", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            // Le pasamos el email que haya escrito, si es que escribió algo
            UC_Soporte panelSoporte = new UC_Soporte(txtEmail.Text.Trim());
            panelSoporte.Dock = DockStyle.Fill;

            // Escuchamos el evento OnCerrar que creamos en el UserControl
            panelSoporte.OnCerrar += Soporte_OnCerrar;

            panel1.Controls.Clear();
            panel1.Controls.Add(panelSoporte);
            panel1.Visible = true;
            panel1.BringToFront();
        }

        private void Soporte_OnCerrar(object sender, EventArgs e)
        {
            // Limpiamos y ocultamos el panel cuando el usuario termina o cancela
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
        }

        private void btnSoporte_Click(object sender, EventArgs e)
        {
            MostrarPanelSoporte();
        }
    }
}