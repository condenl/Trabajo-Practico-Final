// Component/integration checks exclusively against a fresh Perfumeria_Usabilidad_Test_* database.
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using BE;
using BE.Seguridad;
using BLL;
using Servicios;
using UI;
using UI.UserControls;

internal static class TestUsabilidadOperativa
{
    private static int checks;
    private static SqlConnection cn;
    private static T Find<T>(Control root, string name) where T : Control { return (T)root.Controls.Find(name, true).Single(); }
    private static void Assert(bool ok, string text) { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); }
    private static void Click(Control root, string name) {
        var b = Find<Button>(root, name);
        if (!b.Enabled) throw new Exception("Button disabled: " + name);
        typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(b, new object[] { EventArgs.Empty }); Application.DoEvents();
    }
    private static DataTable Query(string sql) {
        using (var cmd = new SqlCommand(sql, cn)) using (var da = new SqlDataAdapter(cmd)) { var t = new DataTable(); da.Fill(t); return t; }
    }
    private static int Count(string table) { return Convert.ToInt32(Query("SELECT COUNT(*) FROM dbo.[" + table + "]").Rows[0][0]); }
    private static object Field(object value, string name) { return value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value); }
    private static void Host(Form host, Control view) { foreach (Control old in host.Controls.Cast<Control>().ToArray()) old.Dispose(); host.Controls.Add(view); host.Show(); Application.DoEvents(); }
    private static void SelectOrder(DataGridView grid, EstadoOrdenCompra state) {
        var row = grid.Rows.Cast<DataGridViewRow>().First(r => (EstadoOrdenCompra)r.Cells["EstadoCodigo"].Value == state);
        grid.CurrentCell = row.Cells["IdOrdenCompra"]; Application.DoEvents();
    }
    private static Usuario Admin(Usuario actor) {
        var admin = (Usuario)actor.Clone(); admin.Permisos = actor.Permisos.ToList();
        foreach (TipoPermiso p in new[] { TipoPermiso.Usuarios_Ver, TipoPermiso.Usuarios_Alta, TipoPermiso.Usuarios_Modificar, TipoPermiso.Usuarios_Baja, TipoPermiso.Roles_Gestion })
            admin.Permisos.Add(new Patente { Nombre = p.ToString(), PermisoAsociado = p });
        return admin;
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try {
            var cs = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString);
            if (!cs.InitialCatalog.StartsWith("Perfumeria_Usabilidad_Test_", StringComparison.Ordinal)) throw new Exception("Solo se admite la base aislada del script test-usabilidad-operativa.ps1.");
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            var owner = new UsuarioBLL().Login("demo.dueno@perfumeria.example", "DemoDueno2026!");
            if (owner == null) throw new Exception("No se pudo iniciar sesión demo.");
            if (args.Length == 0) { Session.Instance.Login(Admin(owner)); Application.Run(new FormPrincipal(Session.Instance.Usuario)); return 0; }
            using (cn = new SqlConnection(cs.ConnectionString)) using (var host = new Form { ClientSize = new Size(1080, 680) }) {
                cn.Open(); int clients = Count("Cliente");
                using (var alta = new FormAltaCliente("test.venta@example.com")) {
                    Click(alta, "btnRegistrarCliente");
                    Assert(Find<Label>(alta, "lblEstadoAltaCliente").Text.Contains("nombre") && Count("Cliente") == clients, "Alta de cliente invalida queda abierta con aviso y sin escrituras");
                    Find<TextBox>(alta, "txtNombre").Text = "Prueba"; Find<TextBox>(alta, "txtApellido").Text = "Usabilidad";
                    Find<CheckBox>(alta, "chkMembresia").Checked = false;
                    Assert(!Find<NumericUpDown>(alta, "nudDescuento").Enabled, "Descuento solo editable si se crea membresia");
                    Find<CheckBox>(alta, "chkMembresia").Checked = true; Find<NumericUpDown>(alta, "nudDescuento").Value = 10;
                    Click(alta, "btnRegistrarCliente");
                    Assert(alta.DialogResult == DialogResult.OK && alta.ClienteCreado.IdCliente > 0 && Count("Cliente") == clients + 1, "Alta separada guarda cliente y membresia reales");
                }
                using (var alta = new FormAltaCliente()) { Click(alta, "btnCancelarCliente"); Assert(Count("Cliente") == clients + 1, "Cancelar alta de cliente no guarda"); }
                var ventas = new UC_Ventas { ModoPrueba = true }; Host(host, ventas);
                Assert(!ventas.Controls.Find("txtNombre", true).Any() && !ventas.Controls.Find("chkMarketing", true).Any(), "Venta mantiene el alta de clientes fuera de la pantalla operativa");
                var productos = Find<ComboBox>(ventas, "cmbProducto"); var floral = productos.Items.Cast<Producto>().Single(p => p.Codigo == "DEMO-FLORAL"); productos.SelectedValue = floral.IdProducto;
                int sales = Count("Venta"), moves = Count("MovimientoStock");
                Click(ventas, "btnAgregarProducto");
                var grid = Find<DataGridView>(ventas, "dgvVenta");
                Assert(grid.ReadOnly && !grid.AllowUserToDeleteRows && grid.Rows.Count == 1, "Carrito de solo lectura con accion explicita para quitar");
                Assert(Count("Venta") == sales && Count("MovimientoStock") == moves, "Vista previa no registra ventas ni movimientos de stock");
                var email = Find<TextBox>(ventas, "txtEmail"); email.Text = "test.venta@example.com"; Click(ventas, "btnBuscarCliente");
                var cotizacion = (Venta)Field(ventas, "cotizacion");
                Assert(cotizacion.Descuento == Math.Round(floral.PrecioVenta * .1m, 2, MidpointRounding.AwayFromZero) && cotizacion.Total == cotizacion.Subtotal - cotizacion.Descuento,
                    "Vista previa aplica membresia con el mismo redondeo de BLL");
                Assert(Find<Label>(ventas, "lblTotalesVenta").Text.Contains(cotizacion.Total.ToString("C2")), "Total visible antes de confirmar");
                email.Text = "otra.persona@example.com";
                Assert(Field(ventas, "clienteActual") == null && ((Venta)Field(ventas, "cotizacion")).Descuento == 0, "Cambiar correo elimina cliente y descuento anteriores");
                email.Text = "test.venta@example.com"; Click(ventas, "btnBuscarCliente"); Click(ventas, "btnQuitarCliente");
                Assert(email.Text == "" && Field(ventas, "clienteActual") == null && grid.Rows.Count == 1, "Venta ocasional conserva carrito y quita identidad del cliente");
                Click(ventas, "btnQuitarProducto"); Assert(grid.Rows.Count == 0 && !Find<Button>(ventas, "btnConfirmarVenta").Enabled, "Quitar ultimo producto pone total en cero y deshabilita confirmacion");
                Find<NumericUpDown>(ventas, "nudCantidad").Value = 999; Click(ventas, "btnAgregarProducto");
                Assert(grid.Rows.Count == 0 && ventas.UltimoError.Contains(floral.Nombre), "Stock insuficiente se avisa con nombre de producto sin agregar una linea invalida");
                Find<NumericUpDown>(ventas, "nudCantidad").Value = 1; Click(ventas, "btnAgregarProducto");
                Query("UPDATE Producto SET PrecioVenta=PrecioVenta+123 WHERE IdProducto=" + floral.IdProducto);
                Click(ventas, "btnConfirmarVenta");
                Assert(Count("Venta") == sales && ventas.UltimoMensaje.Contains("importes cambiaron") && grid.Rows.Count == 1, "Cambio externo de precio requiere revisar el nuevo total antes de guardar");
                email.Text = "test.venta@example.com"; Click(ventas, "btnBuscarCliente"); cotizacion = (Venta)Field(ventas, "cotizacion"); Click(ventas, "btnConfirmarVenta");
                var persisted = Query("SELECT TOP(1) Subtotal,Descuento,Total,IdCliente FROM Venta ORDER BY IdVenta DESC").Rows[0];
                Assert(Count("Venta") == sales + 1 && (decimal)persisted[0] == cotizacion.Subtotal && (decimal)persisted[1] == cotizacion.Descuento && (decimal)persisted[2] == cotizacion.Total, "Importes confirmados coinciden con vista previa y membresia");
                Assert(grid.Rows.Count == 0 && email.Text == "" && Field(ventas, "clienteActual") == null, "Venta finalizada prepara carrito e identidad limpios");
                var compras = new UC_Aprovisionamiento(owner) { ModoPrueba = true }; Host(host, compras);
                var orders = Find<DataGridView>(compras, "dgvOrdenes");
                foreach (var state in new[] { EstadoOrdenCompra.Borrador, EstadoOrdenCompra.PendienteAprobacion, EstadoOrdenCompra.Aprobada, EstadoOrdenCompra.Rechazada, EstadoOrdenCompra.Emitida, EstadoOrdenCompra.RecibidaParcial, EstadoOrdenCompra.Recibida }) {
                    SelectOrder(orders, state);
                    Assert(Find<Button>(compras, "btnEnviarRevision").Enabled == (state == EstadoOrdenCompra.Borrador) && Find<Button>(compras, "btnEmitirOrden").Enabled == (state == EstadoOrdenCompra.Aprobada), "Acciones disponibles segun estado " + state);
                }
                SelectOrder(orders, EstadoOrdenCompra.PendienteAprobacion);
                Assert((string)orders.CurrentRow.Cells["Estado"].Value == "Pendiente de aprobación", "Grilla muestra estados legibles");
                var states = Find<ComboBox>(compras, "cmbFiltroEstado"); states.SelectedIndex = 3; // All, draft, pending, approved.
                Assert(orders.Rows.Count > 0 && orders.Rows.Cast<DataGridViewRow>().All(r => (EstadoOrdenCompra)r.Cells["EstadoCodigo"].Value == EstadoOrdenCompra.Aprobada), "Filtro por estado reduce el listado");
                compras.RutaArchivoPrueba = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ordenes-filtradas.xml"); Click(compras, "btnExportarOrdenesXml");
                Assert(OrdenesXml.Leer(compras.RutaArchivoPrueba).Ordenes.Count == orders.Rows.Count, "Exportacion XML respeta el listado filtrado");
                var providers = Find<ComboBox>(compras, "cmbFiltroProveedor"); providers.SelectedValue = 0;
                Find<CheckBox>(compras, "chkFechasOrdenes").Checked = true; Find<DateTimePicker>(compras, "dtpOrdenDesde").Value = DateTime.Today.AddYears(5);
                Assert(orders.Rows.Count == 0 && !Find<Button>(compras, "btnEmitirOrden").Enabled && !Find<Button>(compras, "btnExportarOrdenesXml").Enabled, "Sin resultados no se habilitan acciones ni exportacion");
                Click(compras, "btnLimpiarFiltrosOrdenes"); Assert(orders.Rows.Count == Count("OrdenCompra"), "Limpiar filtros recupera todas las ordenes");
                int providerId = providers.Items.Cast<Proveedor>().First(p => p.IdProveedor != 0).IdProveedor;
                providers.SelectedValue = providerId;
                string providerName = ((Proveedor)providers.SelectedItem).RazonSocial;
                Assert(orders.Rows.Count > 0 && orders.Rows.Cast<DataGridViewRow>().All(r => (string)r.Cells["Proveedor"].Value == providerName), "Filtro de proveedor usa su nombre");
                Click(compras, "btnActualizarOrdenes"); Assert((int)providers.SelectedValue == providerId, "Actualizar mantiene los filtros disponibles");
                Session.Instance.Login(Admin(owner)); var usuarios = new UC_Usuarios(); Host(host, usuarios);
                Assert(Find<Button>(usuarios, "btnAgregar").Enabled && !Find<Button>(usuarios, "btnModificar").Enabled && !Find<Button>(usuarios, "btnBaja").Enabled, "Nuevo usuario no permite modificar ni desactivar otro usuario");
                Click(usuarios, "btnAgregar"); Assert(!string.IsNullOrWhiteSpace(Find<Label>(usuarios, "lblEstadoUsuarios").Text), "Campos obligatorios de usuario muestran aviso junto al formulario");
                int userCount = Count("Usuarios");
                Find<TextBox>(usuarios, "txtDni").Text = "98123457"; Find<TextBox>(usuarios, "txtNombre").Text = "Alta UI";
                Find<TextBox>(usuarios, "txtApellido").Text = "Usabilidad"; Find<TextBox>(usuarios, "txtEmail").Text = "test.usuario@example.com";
                Find<TextBox>(usuarios, "txtContrasenia").Text = "Usuario2026!Demo"; Click(usuarios, "btnAgregar");
                Assert(Count("Usuarios") == userCount + 1 && Find<TextBox>(usuarios, "txtEmail").Text == "", "Guardar nuevo usuario persiste una cuenta y limpia el formulario");
                var usersGrid = Find<DataGridView>(usuarios, "dgvUsuarios"); var row = usersGrid.Rows.Cast<DataGridViewRow>().Single(r => ((Usuario)r.DataBoundItem).Email == "test.usuario@example.com");
                Assert(Find<TextBox>(usuarios, "txtEmail").Right <= Find<TextBox>(usuarios, "txtEmail").Parent.ClientSize.Width && usersGrid.Right <= usersGrid.Parent.ClientSize.Width,
                    "Formulario y grilla de usuarios caben en el ancho disponible");
                usersGrid.CurrentCell = row.Cells["Email"]; row.Selected = true;
                typeof(UC_Usuarios).GetMethod("dgvUsuarios_CellClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(usuarios, new object[] { usersGrid, new DataGridViewCellEventArgs(row.Cells["Email"].ColumnIndex, row.Index) });
                Assert(!Find<Button>(usuarios, "btnAgregar").Enabled && Find<Button>(usuarios, "btnModificar").Enabled && Find<Label>(usuarios, "lblModoUsuario").Text.Contains("Editar usuario"), "Seleccionar usuario activa modo edicion sin crear duplicados");
                Assert(Find<TextBox>(usuarios, "txtDni").Text == ((Usuario)row.DataBoundItem).Dni, "Seleccion muestra DNI ya descifrado por el mapper");
                string userPassword = ((Usuario)row.DataBoundItem).Password;
                Find<TextBox>(usuarios, "txtEmail").Text = "invalido"; Click(usuarios, "btnModificar");
                Assert(((Usuario)row.DataBoundItem).Email == "test.usuario@example.com" && new UsuarioBLL().BuscarPorEmail("test.usuario@example.com") != null,
                    "Edicion invalida no altera la fila ni los datos persistidos");
                Find<TextBox>(usuarios, "txtEmail").Text = "test.usuario@example.com"; Find<TextBox>(usuarios, "txtNombre").Text = "Editado UI"; Click(usuarios, "btnModificar");
                var saved = new UsuarioBLL().BuscarPorEmail("test.usuario@example.com");
                Assert(saved.Nombre == "Editado UI" && saved.Password == userPassword, "Guardar cambios modifica datos y conserva contraseña vacia");
                row = usersGrid.Rows.Cast<DataGridViewRow>().Single(r => ((Usuario)r.DataBoundItem).Id == saved.Id); usersGrid.CurrentCell = row.Cells["Email"]; row.Selected = true;
                typeof(UC_Usuarios).GetMethod("dgvUsuarios_CellClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(usuarios, new object[] { usersGrid, new DataGridViewCellEventArgs(row.Cells["Email"].ColumnIndex, row.Index) });
                Click(usuarios, "btnNuevoUsuario"); Assert(Find<Button>(usuarios, "btnAgregar").Enabled && usersGrid.CurrentRow == null && Find<TextBox>(usuarios, "txtEmail").Text == "", "Nuevo usuario limpia seleccion y formulario anteriores");
                var restricted = new UC_Usuarios(); Session.Instance.Login(owner); Host(host, restricted);
                Assert(!Find<Button>(restricted, "btnAgregar").Enabled && !Find<Button>(restricted, "btnModificar").Enabled, "Sin permisos administrativos no se habilita alta ni edicion");
                Session.Instance.Login(Admin(owner));
                var target = new Usuario { Nombre = "Antes", Apellido = "Restauracion", Email = "test.audit@example.com", Dni = "98123456" };
                new UsuarioBLL().Insertar(target, "Audit2026!Clave"); target = new UsuarioBLL().BuscarPorEmail(target.Email);
                DAL.AuditoriaDAL.Instancia.GuardarSnapshot("UPDATE", target, owner.Email);
                var version = new AuditoriaBLL().ListarHistorial().First(a => a.Id == target.Id && a.AuditAction == "UPDATE");
                var changed = (Usuario)target.Clone(); changed.Nombre = "Despues"; changed.Email = "test.audit.cambiado@example.com";
                new UsuarioBLL().Modificar(changed, "");
                var current = new UsuarioBLL().ListarTodos().Single(u => u.Id == target.Id);
                using (var preview = new FormRestaurarUsuario(version, current)) {
                    Assert(preview.CantidadCambios == 2 && Find<DataGridView>(preview, "dgvCambiosRestauracion").Rows.Cast<DataGridViewRow>().Any(r => (string)r.Cells["Campo"].Value == "Correo"), "Restauracion muestra valores actuales y futuros de los datos cambiados");
                    Click(preview, "btnCancelarRestauracion");
                    Assert(new UsuarioBLL().ListarTodos().Single(u => u.Id == target.Id).Nombre == "Despues", "Cancelar revision de restauracion no modifica el usuario");
                }
                using (var preview = new FormRestaurarUsuario(version, target))
                    Assert(preview.CantidadCambios == 0 && !Find<Button>(preview, "btnConfirmarRestauracion").Enabled, "Sin diferencias no hay restauracion para confirmar");
                var passwordVersion = new UsuarioAuditoria { Dni = target.Dni, Nombre = target.Nombre, Apellido = target.Apellido, Email = target.Email, Activo = target.Activo, Password = "hash-diferente" };
                using (var preview = new FormRestaurarUsuario(passwordVersion, target))
                    Assert(Find<DataGridView>(preview, "dgvCambiosRestauracion").Rows.Cast<DataGridViewRow>().All(r => !Convert.ToString(r.Cells["Restaurado"].Value).Contains("hash-diferente") && !Convert.ToString(r.Cells["Actual"].Value).Contains(target.Password)), "Revision nunca expone hashes de credenciales");
                new AuditoriaBLL().HacerRollback(version);
                var restored = new UsuarioBLL().ListarTodos().Single(u => u.Id == target.Id);
                Assert(restored.Nombre == target.Nombre && restored.Email == target.Email && restored.Password == target.Password, "Restauracion por ID recupera nombre y correo conservando credencial historica");
                Assert(new UsuarioBLL().VerificarIntegridad().Count == 0, "Restauracion conserva integridad DVH y DVV");
                var audit = new UC_AuditoriaUsuarios(); Host(host, audit);
                Assert(!Find<Button>(audit, "btnRollback").Enabled && Find<Label>(audit, "lblEstadoAuditoria").Text.Contains("revise"), "Auditoria exige seleccionar y revisar una version antes de restaurar");
                Find<TextBox>(audit, "txtBuscar").Text = "test.audit@example.com"; Click(audit, "btnBuscar");
                int matches = Find<DataGridView>(audit, "dgvHistorial").Rows.Count; Click(audit, "btnActualizarAuditoria");
                Assert(matches > 0 && Find<DataGridView>(audit, "dgvHistorial").Rows.Count == matches, "Actualizar auditoria conserva la busqueda aplicada");
                Console.WriteLine("RESULTADO: " + checks + " verificaciones OK; BASE=" + cs.InitialCatalog);
            }
            return 0;
        } catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { new UsuarioBLL().Logout(); }
    }
}
