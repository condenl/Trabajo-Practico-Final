using System;
using System.Collections.Generic;
using System.Linq;
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
        private Button accesoActivo;
        private readonly Label sesionActiva = new Label { Name = "lblSesionActiva", AutoEllipsis = true,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 10, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = SystemInformation.HighContrast ? System.Drawing.SystemColors.Control : TemaAplicacion.Elevado,
            Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont, System.Drawing.FontStyle.Bold) };

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
            sesionActiva.SetBounds(panel1.Left, panel1.Top, panel1.Width, 32);
            panel1.Top += 38; panel1.Height -= 38;
            Controls.Add(sesionActiva);
            TemaAplicacion.Aplicar(this);
            if (!SystemInformation.HighContrast) BackColor = TemaAplicacion.Navegacion;
            foreach (Button boton in new[] { btnBitacora, btnUsuarios, btnRoles, btnSoporteTecnico,
                btnTraducciones, btnAuditoria, btnVentas, btnAprovisionamiento, btnCerrarSesion })
                TemaAplicacion.AplicarNavegacion(boton, false);
        }

        private void MostrarVista(Func<UserControl> crearVista, Button acceso)
        {
            if (!PuedeAbandonarVista()) return;
            var vista = crearVista();
            foreach (Control anterior in new List<Control>(ObtenerVistas())) anterior.Dispose();
            vista.Dock = DockStyle.Fill; panel1.Controls.Add(vista); vista.Show();
            if (accesoActivo != null) TemaAplicacion.AplicarNavegacion(accesoActivo, false);
            accesoActivo = acceso; TemaAplicacion.AplicarNavegacion(accesoActivo, true);
        }

        private bool PuedeAbandonarVista()
        {
            foreach (Control vista in panel1.Controls)
                if (vista is IVistaConCambiosPendientes && ((IVistaConCambiosPendientes)vista).TieneCambiosPendientes)
                    return ConfirmarDescartePendiente();
            return true;
        }

        protected virtual bool ConfirmarDescartePendiente()
        {
            return MessageBox.Show(this, "Hay datos sin guardar en esta vista. ¿Desea descartarlos y continuar?", "Datos sin guardar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
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
            btnAuditoria.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Auditoria_Ver);
            btnSoporteTecnico.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Soporte_Gestion);
            btnVentas.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Ventas_Registrar);
            btnAprovisionamiento.Visible =
                _usuarioLogueado.TienePermiso(TipoPermiso.Aprovisionamiento_Gestion) ||
                _usuarioLogueado.TienePermiso(TipoPermiso.Contabilidad_AprobarCompras) ||
                _usuarioLogueado.TienePermiso(TipoPermiso.Inventario_Recepcionar);
            ActualizarSesionActiva();
        }

        private void ActualizarSesionActiva()
        {
            if (_usuarioLogueado == null) { sesionActiva.Text = ""; return; }
            string roles = string.Join(", ", _usuarioLogueado.Permisos.OfType<Familia>().Select(r => r.Nombre).Distinct());
            sesionActiva.Text = _usuarioLogueado.NombreCompleto + (roles.Length == 0 ? "" : "  ·  " + roles) + "  ·  " + _usuarioLogueado.Email;
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
                this.Text = $"{title} - {_usuarioLogueado.NombreCompleto}";
                ActualizarSesionActiva();
            }
        }

        private void btnBitacora_Click(object sender, EventArgs e)
        {
            MostrarVista(delegate { return new UC_Bitacora(); }, btnBitacora);
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
            MostrarVista(delegate { return new UC_Usuarios(); }, btnUsuarios);
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            MostrarVista(delegate { return new UC_Roles(); }, btnRoles);
        }

        private void btnSoporteTecnico_Click(object sender, EventArgs e)
        {
            MostrarVista(delegate { return new UC_SoporteAdmin(); }, btnSoporteTecnico);
        }

        private void btnTraducciones_Click(object sender, EventArgs e)
        {
            MostrarVista(delegate { return new UC_Traducciones(); }, btnTraducciones);
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
            if (!PuedeAbandonarVista()) { e.Cancel = true; return; }
            GestorIdioma.Instancia.Desuscribir(this);
        }

        private void btnAuditoria_Click(object sender, EventArgs e)
        {
            MostrarVista(delegate { return new UC_AuditoriaUsuarios(); }, btnAuditoria);
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            MostrarVista(delegate { return new UC_Ventas(); }, btnVentas);
        }

        private void btnAprovisionamiento_Click(object sender, EventArgs e)
        {
            MostrarVista(delegate { return new UC_Aprovisionamiento(_usuarioLogueado); }, btnAprovisionamiento);
        }
    }
}
