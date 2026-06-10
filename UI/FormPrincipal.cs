using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using UI.UserControls;
using BE.Seguridad;
using BE.Interfaces;
using BLL;
using Servicios;

namespace UI
{
    public partial class FormPrincipal : Form, IIdiomaObserver
    {
        private Usuario _usuarioLogueado;
        private readonly IdiomaBLL idiomaBLL;
        private readonly TraduccionBLL traduccionBLL;

        public FormPrincipal(Usuario usuario)
        {
            InitializeComponent();
            _usuarioLogueado = usuario;
            idiomaBLL = new IdiomaBLL();
            traduccionBLL = new TraduccionBLL();

            ConfigurarAccesos();
        }

        public FormPrincipal()
        {
            InitializeComponent();
            idiomaBLL = new IdiomaBLL();
            traduccionBLL = new TraduccionBLL();
        }

        private void ConfigurarAccesos()
        {
            if (_usuarioLogueado == null) return;

            btnUsuarios.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Usuarios_Ver);
            btnRoles.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Roles_Gestion);
            btnBitacora.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.VerReportes);
        }

        private void ConfigurarIdiomas()
        {
            cmbIdioma.DataSource = null;
            cmbIdioma.DataSource = idiomaBLL.Listar();
            cmbIdioma.DisplayMember = "Nombre";

            GestorIdioma.Instancia.Suscribir(this);

            if (GestorIdioma.Instancia.IdiomaActual != null)
            {
                List<Idioma> idiomas = (List<Idioma>)cmbIdioma.DataSource;
                cmbIdioma.SelectedItem = idiomas.Find(i => i.Id == GestorIdioma.Instancia.IdiomaActual.Id);
            }
            
            ActualizarTextos();
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
            
            if (_usuarioLogueado != null)
            {
                string title = traduccionBLL.Traducir("main_form_title");
                this.Text = $"{title} - {traduccionBLL.Traducir("login_user")}: {_usuarioLogueado.Nombre}";
            }
        }

        private void btnBitacora_Click(object sender, EventArgs e)
        {
            UC_Bitacora bitacora = new UC_Bitacora();
            panel1.Controls.Clear();
            panel1.Controls.Add(bitacora);
            bitacora.Show();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show(
                traduccionBLL.Traducir("msg_confirm_logout"),
                traduccionBLL.Traducir("title_confirm_logout"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            UC_Usuarios usuarios = new UC_Usuarios();
            panel1.Controls.Clear();
            panel1.Controls.Add(usuarios);
            usuarios.Show();
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            UC_Roles roles = new UC_Roles();
            panel1.Controls.Clear();
            panel1.Controls.Add(roles);
            roles.Show();
        }

        private void btnSoporteTecnico_Click(object sender, EventArgs e)
        {
            UC_SoporteAdmin soporteadmin = new UC_SoporteAdmin();
            panel1.Controls.Clear();
            panel1.Controls.Add(soporteadmin);
            soporteadmin.Show();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            ConfigurarIdiomas();
        }

        private void cmbIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIdioma.SelectedItem != null)
            {
                Idioma seleccionado = (Idioma)cmbIdioma.SelectedItem;
                if (GestorIdioma.Instancia.IdiomaActual == null || GestorIdioma.Instancia.IdiomaActual.Id != seleccionado.Id)
                {
                    GestorIdioma.Instancia.SeleccionarIdioma(seleccionado);
                }
            }
        }

        private void FormPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            GestorIdioma.Instancia.Desuscribir(this);
        }
    }
}
