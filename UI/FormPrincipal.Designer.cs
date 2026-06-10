namespace UI
{
    partial class FormPrincipal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBitacora = new System.Windows.Forms.Button();
            this.btnCerrarSesion = new System.Windows.Forms.Button();
            this.btnUsuarios = new System.Windows.Forms.Button();
            this.btnRoles = new System.Windows.Forms.Button();
            this.btnSoporteTecnico = new System.Windows.Forms.Button();
            this.cmbIdioma = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(183, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1325, 599);
            this.panel1.TabIndex = 0;
            // 
            // btnBitacora
            // 
            this.btnBitacora.Location = new System.Drawing.Point(12, 12);
            this.btnBitacora.Name = "btnBitacora";
            this.btnBitacora.Size = new System.Drawing.Size(120, 43);
            this.btnBitacora.TabIndex = 0;
            this.btnBitacora.Tag = "menu_bitacora";
            this.btnBitacora.Text = "Bitacora";
            this.btnBitacora.UseVisualStyleBackColor = true;
            this.btnBitacora.Click += new System.EventHandler(this.btnBitacora_Click);
            // 
            // btnCerrarSesion
            // 
            this.btnCerrarSesion.Location = new System.Drawing.Point(12, 568);
            this.btnCerrarSesion.Name = "btnCerrarSesion";
            this.btnCerrarSesion.Size = new System.Drawing.Size(120, 43);
            this.btnCerrarSesion.TabIndex = 1;
            this.btnCerrarSesion.Tag = "menu_logout";
            this.btnCerrarSesion.Text = "Cerrar Sesion";
            this.btnCerrarSesion.UseVisualStyleBackColor = true;
            this.btnCerrarSesion.Click += new System.EventHandler(this.btnCerrarSesion_Click);
            // 
            // btnUsuarios
            // 
            this.btnUsuarios.Location = new System.Drawing.Point(12, 76);
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Size = new System.Drawing.Size(120, 43);
            this.btnUsuarios.TabIndex = 2;
            this.btnUsuarios.Tag = "menu_usuarios";
            this.btnUsuarios.Text = "Usuarios";
            this.btnUsuarios.UseVisualStyleBackColor = true;
            this.btnUsuarios.Click += new System.EventHandler(this.btnUsuarios_Click);
            // 
            // btnRoles
            // 
            this.btnRoles.Location = new System.Drawing.Point(12, 146);
            this.btnRoles.Name = "btnRoles";
            this.btnRoles.Size = new System.Drawing.Size(120, 43);
            this.btnRoles.TabIndex = 3;
            this.btnRoles.Tag = "menu_roles";
            this.btnRoles.Text = "Roles";
            this.btnRoles.UseVisualStyleBackColor = true;
            this.btnRoles.Click += new System.EventHandler(this.btnRoles_Click);
            // 
            // btnSoporteTecnico
            // 
            this.btnSoporteTecnico.Location = new System.Drawing.Point(12, 220);
            this.btnSoporteTecnico.Name = "btnSoporteTecnico";
            this.btnSoporteTecnico.Size = new System.Drawing.Size(120, 43);
            this.btnSoporteTecnico.TabIndex = 4;
            this.btnSoporteTecnico.Tag = "menu_soporte_admin";
            this.btnSoporteTecnico.Text = "Soporte Tecnico";
            this.btnSoporteTecnico.UseVisualStyleBackColor = true;
            this.btnSoporteTecnico.Click += new System.EventHandler(this.btnSoporteTecnico_Click);
            // 
            // cmbIdioma
            // 
            this.cmbIdioma.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIdioma.FormattingEnabled = true;
            this.cmbIdioma.Location = new System.Drawing.Point(12, 530);
            this.cmbIdioma.Name = "cmbIdioma";
            this.cmbIdioma.Size = new System.Drawing.Size(121, 21);
            this.cmbIdioma.TabIndex = 5;
            this.cmbIdioma.SelectedIndexChanged += new System.EventHandler(this.cmbIdioma_SelectedIndexChanged);
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1520, 623);
            this.Controls.Add(this.cmbIdioma);
            this.Controls.Add(this.btnSoporteTecnico);
            this.Controls.Add(this.btnRoles);
            this.Controls.Add(this.btnUsuarios);
            this.Controls.Add(this.btnCerrarSesion);
            this.Controls.Add(this.btnBitacora);
            this.Controls.Add(this.panel1);
            this.Name = "FormPrincipal";
            this.Text = "FormPrincipal";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPrincipal_FormClosing);
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnBitacora;
        private System.Windows.Forms.Button btnCerrarSesion;
        private System.Windows.Forms.Button btnUsuarios;
        private System.Windows.Forms.Button btnRoles;
        private System.Windows.Forms.Button btnSoporteTecnico;
        private System.Windows.Forms.ComboBox cmbIdioma;
    }
}