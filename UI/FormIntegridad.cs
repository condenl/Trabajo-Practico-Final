using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using BE;
using BLL;
using BE.Interfaces;
using Servicios;

namespace UI
{
    public partial class FormIntegridad : Form, IIdiomaObserver
    {
        private readonly Usuario _usuario;
        private List<string> _errores;
        private readonly UsuarioBLL _usuarioBLL;
        private readonly TraduccionBLL _traduccionBLL;

        public FormIntegridad(Usuario usuario, List<string> errores)
        {
            InitializeComponent();
            _usuario = usuario;
            _errores = errores;
            _usuarioBLL = new UsuarioBLL();
            _traduccionBLL = new TraduccionBLL();

            // Set tags for dynamic translations
            lblTitulo.Tag = "integridad_lbl_titulo";
            lblSugerencia.Tag = "integridad_lbl_sugerencia";
            btnRecalcular.Tag = "integridad_btn_recalcular";
            btnRestaurar.Tag = "integridad_btn_restaurar";
            btnSalir.Tag = "integridad_btn_salir";

            GestorIdioma.Instancia.Suscribir(this);
            TemaAplicacion.Aplicar(this);
        }

        private void FormIntegridad_Load(object sender, EventArgs e)
        {
            ActualizarTextos();
            ActualizarListaErrores();
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
                    control.Text = _traduccionBLL.Traducir(control.Tag.ToString());
                }
            }
            this.Text = _traduccionBLL.Traducir("integridad_form_title");
            MostrarRutaBackup();
        }

        private void ActualizarListaErrores()
        {
            txtErrores.Clear();
            if (_errores != null && _errores.Count > 0)
            {
                txtErrores.Text = string.Join(Environment.NewLine + Environment.NewLine, _errores);
            }
            else
            {
                txtErrores.Text = _traduccionBLL.Traducir("integridad_msg_no_errors");
            }
        }

        private string ObtenerRutaBackup()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // standard layout: UI/bin/Debug/ -> SQL
            string path = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\SQL"));
            if (!Directory.Exists(path))
            {
                // alternate layout: UI/bin/ -> SQL
                path = Path.GetFullPath(Path.Combine(baseDir, @"..\..\SQL"));
            }
            if (!Directory.Exists(path))
            {
                path = baseDir;
            }
            return path;
        }

        private void MostrarRutaBackup()
        {
            string ruta = ObtenerRutaBackup();
            string prefix = _traduccionBLL.Traducir("integridad_lbl_sugerencia");
            lblSugerencia.Text = $"{prefix}\n{ruta}";
        }

        private void btnRecalcular_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _usuarioBLL.RecalcularDV();

                _errores = _usuarioBLL.VerificarIntegridad();
                ActualizarListaErrores();

                if (_errores.Count == 0)
                {
                    string successMsg = _traduccionBLL.Traducir("integridad_msg_recalcular_success");
                    string successTitle = _traduccionBLL.Traducir("lbl_success");
                    MessageBox.Show(successMsg, successTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    BitacoraBLL.Instancia.Registrar(new EventoBitacora(_usuario.Email, "RESTORE_INTEGRIDAD", "Se ejecutó el recalculo de DVH/DVV corrigiendo la integridad de la base de datos."));
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    string warnMsg = _traduccionBLL.Traducir("integridad_msg_recalcular_warning");
                    string warnTitle = _traduccionBLL.Traducir("lbl_warning");
                    MessageBox.Show(warnMsg, warnTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                string errMsg = _traduccionBLL.Traducir("integridad_msg_recalcular_error") + ": " + ex.Message;
                string errTitle = _traduccionBLL.Traducir("lbl_error");
                MessageBox.Show(errMsg, errTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "SQL Server Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
                ofd.InitialDirectory = ObtenerRutaBackup();
                ofd.Title = _traduccionBLL.Traducir("integridad_title_select_backup");

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        _usuarioBLL.RestaurarBaseDeDatos(ofd.FileName);

                        // Se debe re-verificar la integridad tras el restore
                        _errores = _usuarioBLL.VerificarIntegridad();
                        ActualizarListaErrores();

                        if (_errores.Count == 0)
                        {
                            string successMsg = _traduccionBLL.Traducir("integridad_msg_restore_success");
                            string successTitle = _traduccionBLL.Traducir("lbl_success");
                            MessageBox.Show(successMsg, successTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            BitacoraBLL.Instancia.Registrar(new EventoBitacora(_usuario.Email, "RESTORE", $"Se restauró la base de datos exitosamente desde el backup: {Path.GetFileName(ofd.FileName)}"));
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            string warnMsg = _traduccionBLL.Traducir("integridad_msg_restore_warning");
                            string warnTitle = _traduccionBLL.Traducir("lbl_warning");
                            MessageBox.Show(warnMsg, warnTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errMsg = _traduccionBLL.Traducir("integridad_msg_restore_error") + ": " + ex.Message + "\n\n" + _traduccionBLL.Traducir("integridad_msg_restore_perm_error");
                        string errTitle = _traduccionBLL.Traducir("lbl_error");
                        MessageBox.Show(errMsg, errTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            GestorIdioma.Instancia.Desuscribir(this);
        }
    }
}
