using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Control panelAcceso;

        public Login()
        {
            InitializeComponent();
            visitanteBLL = new UsuarioBLL();
            idiomaBLL = new IdiomaBLL();
            traduccionBLL = new TraduccionBLL();
            ConfigurarFormulario();
            TemaAplicacion.Aplicar(this);
        }

        private void ConfigurarFormulario()
        {
            if (txtPassword != null)
                txtPassword.UseSystemPasswordChar = true;

            panel1.Controls.Clear();
            panel1.Visible = false; panel1.Dock = DockStyle.Fill; panel1.AutoScroll = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(660, 540); MinimumSize = new Size(520, 430);
            var acceso = new TableLayoutPanel { Name = "layoutAcceso", Dock = DockStyle.Top, AutoSize = true,
                ColumnCount = 2, RowCount = 5, Padding = new Padding(24, 18, 24, 18) };
            acceso.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); acceso.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < 5; i++) acceso.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            label3.Font = new Font("Segoe UI", 16, FontStyle.Bold); label3.Text = "Perfumería";
            label4.Text = "Gestioná compras, inventario, ventas y proveedores. Iniciá sesión para continuar.";
            label6.Text = "Correo electrónico"; label1.Text = "Contraseña";
            label2.Text = "¿Todavía no tenés una cuenta?"; label5.Text = "¿Necesitás ayuda para acceder?";
            cmbIdioma.Width = 145; cmbIdioma.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            acceso.Controls.Add(label3, 0, 0); acceso.Controls.Add(cmbIdioma, 1, 0);
            acceso.Controls.Add(label4, 0, 1); acceso.SetColumnSpan(label4, 2);
            label4.Margin = new Padding(0, 8, 0, 12);
            var campos = new TableLayoutPanel { Name = "camposAcceso", Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, TabIndex = 0 };
            int fila = 0;
            foreach (Control campo in new Control[] { label6, txtEmail, label1, txtPassword, btnIniciarSesion }) {
                campos.RowCount++; campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                campo.Margin = new Padding(0, 4, 0, 8); campos.Controls.Add(campo, 0, fila++);
            }
            txtEmail.Dock = txtPassword.Dock = DockStyle.Top;
            btnIniciarSesion.AutoSize = true; btnIniciarSesion.Size = new Size(145, 36); btnIniciarSesion.Padding = new Padding(8, 3, 8, 3);
            acceso.Controls.Add(campos, 0, 2); acceso.SetColumnSpan(campos, 2);
            int filaAccion = 3;
            foreach (var par in new[] { new Control[] { label2, btnRegistro }, new Control[] { label5, btnSoporte } }) {
                var acciones = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, TabIndex = filaAccion };
                par[0].Margin = new Padding(0, 12, 12, 8);
                var boton = (Button)par[1]; boton.AutoSize = true; boton.Size = new Size(130, 36); boton.Padding = new Padding(6, 3, 6, 3);
                acciones.Controls.Add(par[0]); acciones.Controls.Add(boton);
                acceso.Controls.Add(acciones, 0, filaAccion++); acceso.SetColumnSpan(acciones, 2);
            }
            acceso.SizeChanged += delegate {
                var limite = new Size(Math.Max(220, acceso.ClientSize.Width - acceso.Padding.Horizontal), 0);
                label4.MaximumSize = label2.MaximumSize = label5.MaximumSize = limite;
            };
            panelAcceso = new Panel { Name = "panelAcceso", Dock = DockStyle.Fill, AutoScroll = true, TabIndex = 0 };
            panelAcceso.Controls.Add(acceso); Controls.Clear(); Controls.Add(panelAcceso); Controls.Add(panel1);
            txtEmail.TabIndex = 0; txtPassword.TabIndex = 1; btnIniciarSesion.TabIndex = 2;
            btnRegistro.TabIndex = 3; btnSoporte.TabIndex = 4; cmbIdioma.TabIndex = 5; panel1.TabIndex = 6;
            AcceptButton = btnIniciarSesion;
            ActiveControl = txtEmail;
        }

        private void ConfigurarIdiomas()
        {
            cmbIdioma.DataSource = null;
            cmbIdioma.DataSource = idiomaBLL.Listar();
            cmbIdioma.DisplayMember = "Nombre";
            
            GestorIdioma.Instancia.Suscribir(this);

            List<Idioma> idiomas = (List<Idioma>)cmbIdioma.DataSource;
            
            Idioma idiomaSeleccionado = null;
            int persistedId = GestorIdioma.Instancia.ObtenerIdiomaPersistido();
            if (persistedId > 0 && idiomas != null)
            {
                idiomaSeleccionado = idiomas.Find(i => i.Id == persistedId);
            }

            if (idiomaSeleccionado == null && idiomas != null)
            {
                idiomaSeleccionado = idiomas.Find(i => i.EsDefault);
            }

            if (idiomaSeleccionado != null)
            {
                cmbIdioma.SelectedItem = idiomaSeleccionado;
                GestorIdioma.Instancia.SeleccionarIdioma(idiomaSeleccionado);
            }
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
            ActualizarTextosHijos(this);
            string titulo = traduccionBLL.Traducir("login_form_title");
            this.Text = string.IsNullOrWhiteSpace(titulo) || titulo == "login_form_title" || titulo == "[login_form_title]" ? "Perfumería" : titulo;
        }

        private void ActualizarTextosHijos(Control padre)
        {
            foreach (Control hijo in padre.Controls) {
                if (hijo.Tag != null) {
                    string etiqueta = hijo.Tag.ToString(); string texto = traduccionBLL.Traducir(etiqueta);
                    if (!string.IsNullOrWhiteSpace(texto) && texto != etiqueta && texto != "[" + etiqueta + "]") hijo.Text = texto;
                }
                ActualizarTextosHijos(hijo);
            }
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
                var errores = new UsuarioBLL().VerificarIntegridad();
                if (errores.Count > 0)
                {
                    if (usuarioLogueado.TienePermiso(BE.Seguridad.TipoPermiso.VerReportes))
                    {
                        FormIntegridad formIntegridad = new FormIntegridad(usuarioLogueado, errores);
                        if (formIntegridad.ShowDialog() == DialogResult.OK)
                        {
                            NavegarAlSistema(usuarioLogueado);
                        }
                        else
                        {
                            visitanteBLL.Logout();
                        }
                    }
                    else
                    {
                        // Para el resto de usuarios, cuando hay un error de integridad en la base de datos al momento de login,
                        // pueden seguir utilizando la aplicacion con normalidad.
                        NavegarAlSistema(usuarioLogueado);
                    }
                }
                else
                {
                    NavegarAlSistema(usuarioLogueado);
                }
            }
            else
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_invalid_credentials"), traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavegarAlSistema(Usuario usuario)
        {
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
            this.Show();
            ActualizarTextos();
            AcceptButton = btnIniciarSesion; ActiveControl = txtEmail;
        }

        private void MostrarRegistro()
        {
            UC_Registro registro = new UC_Registro();
            registro.AutoScroll = true; panelAcceso.Enabled = false;
            registro.Dock = DockStyle.Fill;
            registro.OnRegresar += Registro_OnRegresar;

            panel1.Controls.Clear();
            panel1.Controls.Add(registro);
            panel1.Visible = true;
            panel1.BringToFront();
            AcceptButton = null;
        }

        private void MostrarPanelSoporte()
        {
            UC_Soporte panelSoporte = new UC_Soporte(txtEmail.Text.Trim());
            panelSoporte.AutoScroll = true; panelAcceso.Enabled = false;
            panelSoporte.Dock = DockStyle.Fill;
            panelSoporte.OnCerrar += Soporte_OnCerrar;

            panel1.Controls.Clear();
            panel1.Controls.Add(panelSoporte);
            panel1.Visible = true;
            panel1.BringToFront();
            AcceptButton = null;
        }

        private void Soporte_OnCerrar(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel1.Visible = false;
            panelAcceso.Enabled = true;
            AcceptButton = btnIniciarSesion; ActiveControl = txtEmail;
        }

        private void Registro_OnRegresar(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel1.Visible = false;
            panelAcceso.Enabled = true;
            AcceptButton = btnIniciarSesion; ActiveControl = txtEmail;
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
