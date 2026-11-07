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
            ConfigurarVentana();
            _usuarioLogueado = usuario;
            idiomaBLL = new IdiomaBLL();
            traduccionBLL = new TraduccionBLL();

            ConfigurarAccesos();
        }

        public FormPrincipal()
        {
            InitializeComponent();
            ConfigurarVentana();
            idiomaBLL = new IdiomaBLL();
            traduccionBLL = new TraduccionBLL();
        }

        private void ConfigurarVentana()
        {
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new System.Drawing.Size(1000, 570);
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cmbIdioma.Anchor = btnCerrarSesion.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            var area = Screen.FromControl(this).WorkingArea;
            ClientSize = new System.Drawing.Size(Math.Min(1520, area.Width - 32), Math.Min(760, area.Height - 64));
        }

        private void MostrarVista(UserControl vista)
        {
            foreach (Control anterior in new List<Control>(ObtenerVistas())) anterior.Dispose();
            vista.Dock = DockStyle.Fill; panel1.Controls.Add(vista); vista.Show();
        }

        private IEnumerable<Control> ObtenerVistas()
        {
            foreach (Control control in panel1.Controls) yield return control;
        }

        private void ConfigurarAccesos()
        {
            if (_usuarioLogueado == null) return;

            btnUsuarios.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Usuarios_Ver);
            btnRoles.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Roles_Gestion);
            btnBitacora.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.VerReportes);
            btnTraducciones.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Traduccion_Gestion);
            btnVentas.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Ventas_Registrar);
            btnAprovisionamiento.Visible =
                _usuarioLogueado.TienePermiso(TipoPermiso.Aprovisionamiento_Gestion) ||
                _usuarioLogueado.TienePermiso(TipoPermiso.Contabilidad_AprobarCompras) ||
                _usuarioLogueado.TienePermiso(TipoPermiso.Inventario_Recepcionar);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Visible includes parent visibility; arrange after the form is shown
            // and WinForms has applied font/DPI scaling and actor permissions.
            int siguiente = btnBitacora.Top;
            int separacion = Math.Max(6, btnAprovisionamiento.Height / 4);
            SuspendLayout();
            foreach (Button boton in new[] { btnBitacora, btnUsuarios, btnRoles, btnSoporteTecnico,
                btnTraducciones, btnAuditoria, btnVentas, btnAprovisionamiento })
            {
                if (!boton.Visible) continue;
                boton.Top = siguiente;
                siguiente += boton.Height + separacion;
            }
            ResumeLayout(false);
            if (panel1.Controls.Count == 0)
            {
                if (btnAprovisionamiento.Visible) btnAprovisionamiento_Click(this, EventArgs.Empty);
                else if (btnVentas.Visible) btnVentas_Click(this, EventArgs.Empty);
            }
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
            cmbIdioma.SelectedIndexChanged -= cmbIdioma_SelectedIndexChanged;

            cmbIdioma.DataSource = null;
            cmbIdioma.DataSource = idiomaBLL.Listar();
            cmbIdioma.DisplayMember = "Nombre";

            if (GestorIdioma.Instancia.IdiomaActual != null)
            {
                List<Idioma> idiomas = (List<Idioma>)cmbIdioma.DataSource;
                if (idiomas != null)
                {
                    cmbIdioma.SelectedItem = idiomas.Find(i => i.Id == GestorIdioma.Instancia.IdiomaActual.Id);
                }
            }

            cmbIdioma.SelectedIndexChanged += cmbIdioma_SelectedIndexChanged;

            ActualizarTextos();
        }

        private void ActualizarTextos()
        {
            foreach (Control control in this.Controls)
            {
                if (control.Tag != null)
                {
                    string etiqueta = control.Tag.ToString();
                    string texto = traduccionBLL.Traducir(etiqueta);
                    if (!string.IsNullOrWhiteSpace(texto) && texto != etiqueta && texto != "[" + etiqueta + "]")
                        control.Text = texto;
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
            MostrarVista(bitacora);
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
            MostrarVista(usuarios);
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            UC_Roles roles = new UC_Roles();
            MostrarVista(roles);
        }

        private void btnSoporteTecnico_Click(object sender, EventArgs e)
        {
            UC_SoporteAdmin soporteadmin = new UC_SoporteAdmin();
            MostrarVista(soporteadmin);
        }

        private void btnTraducciones_Click(object sender, EventArgs e)
        {
            UC_Traducciones traducciones = new UC_Traducciones();
            MostrarVista(traducciones);
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

        private void btnAuditoria_Click(object sender, EventArgs e)
        {
            UC_AuditoriaUsuarios auditoria = new UC_AuditoriaUsuarios();
            MostrarVista(auditoria);
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            UC_Ventas ventas = new UC_Ventas();
            MostrarVista(ventas);
        }

        private void btnAprovisionamiento_Click(object sender, EventArgs e)
        {
            UC_Aprovisionamiento aprovisionamiento = new UC_Aprovisionamiento(_usuarioLogueado);
            MostrarVista(aprovisionamiento);
        }
    }
}
