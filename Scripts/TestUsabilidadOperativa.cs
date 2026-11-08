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
    private static void Host(Form host, Control view) { foreach (Control old in host.Controls.Cast<Control>().ToArray()) old.Dispose(); host.Controls.Add(view); TemaAplicacion.Aplicar(view); host.Show(); Application.DoEvents(); }
    private static void SelectOrder(DataGridView grid, EstadoOrdenCompra state) {
        var row = grid.Rows.Cast<DataGridViewRow>().First(r => (EstadoOrdenCompra)r.Cells["EstadoCodigo"].Value == state);
        grid.CurrentCell = row.Cells["IdOrdenCompra"]; Application.DoEvents();
    }
    private static Usuario Admin(Usuario actor) {
        var admin = (Usuario)actor.Clone(); admin.Permisos = actor.Permisos.ToList();
        foreach (TipoPermiso p in new[] { TipoPermiso.Usuarios_Ver, TipoPermiso.Usuarios_Alta, TipoPermiso.Usuarios_Modificar, TipoPermiso.Usuarios_Baja, TipoPermiso.Roles_Gestion,
            TipoPermiso.Auditoria_Ver, TipoPermiso.Auditoria_Restaurar, TipoPermiso.Soporte_Gestion })
            admin.Permisos.Add(new Patente { Nombre = p.ToString(), PermisoAsociado = p });
        return admin;
    }
    private sealed class MenuPrueba : FormPrincipal {
        public bool Descartar; public int Consultas;
        public MenuPrueba(Usuario actor) : base(actor) { }
        protected override bool ConfirmarDescartePendiente() { Consultas++; return Descartar; }
    }
    private static void Denegado(Action accion, string texto)
    { bool rechazo = false; try { accion(); } catch (UnauthorizedAccessException) { rechazo = true; } Assert(rechazo, texto); }
    private static void ComprobarRestricciones(FormPrincipal menu, string actor)
    {
        Assert(!Find<Button>(menu, "btnAuditoria").Visible && !Find<Button>(menu, "btnSoporteTecnico").Visible, "Menu administrativo oculto para " + actor);
        Assert(menu.Text.EndsWith(Session.Instance.Usuario.NombreCompleto) && !menu.Text.Contains("Correo electrónico:"), "El titulo identifica al actor sin reutilizar la etiqueta de correo: " + actor);
        Denegado(delegate { new AuditoriaBLL().ListarHistorial(); }, actor + " no consulta auditoria por BLL");
        Denegado(delegate { new AuditoriaBLL().HacerRollback(null); }, actor + " no restaura por BLL");
        Denegado(delegate { new SoporteBLL().ListarSolicitudes(); }, actor + " no lista solicitudes administrativas");
        Denegado(delegate { new SoporteBLL().ModificarSolicitudPorAdmin(null); }, actor + " no modifica solicitudes administrativas");
        Denegado(delegate { using (var vista = new UC_AuditoriaUsuarios()) { } }, actor + " no fuerza la apertura de auditoria");
        Denegado(delegate { using (var vista = new UC_SoporteAdmin()) { } }, actor + " no fuerza la apertura de soporte administrativo");
    }
    private static void ComprobarMigracion()
    {
        Assert(Query("SELECT Id FROM Seguridad_Componente WHERE PermisoAsociado IN ('Auditoria_Ver','Auditoria_Restaurar','Soporte_Gestion')").Rows.Count == 3, "Migracion crea tres permisos administrativos");
        var idiomas = new IdiomaBLL().Listar(); var traducciones = new TraduccionBLL();
        Assert(traducciones.ObtenerTraduccion(idiomas.First(i => i.Nombre == "Español"), "login_title").Texto == "Perfumería" &&
            traducciones.ObtenerTraduccion(idiomas.First(i => i.Nombre == "English"), "login_title").Texto == "Perfumery", "La distribucion identifica Perfumeria en ambos idiomas");
        var antes = Query("SELECT COUNT(*) AS Cantidad FROM Seguridad_Familia_Hijo").Rows[0][0];
        string dvv = Convert.ToString(Query("SELECT Valor FROM DVV WHERE NombreEntidad='Usuario'").Rows[0][0]);
        Exec("UPDATE Traduccion SET Texto=N'Acceso personalizado de prueba' WHERE NombreEtiqueta='login_title' AND IdIdioma IN (SELECT Id FROM Idioma WHERE Nombre='English')");
        string sql = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Iteracion3_Pulido_Demo.sql"));
        for (int repeticion=0; repeticion<2; repeticion++) foreach (string batch in System.Text.RegularExpressions.Regex.Split(sql, @"(?im)^\s*GO\s*(?:--.*)?$")) if (!string.IsNullOrWhiteSpace(batch)) Exec(batch);
        Assert(Convert.ToInt32(antes) == Convert.ToInt32(Query("SELECT COUNT(*) FROM Seguridad_Familia_Hijo").Rows[0][0]) &&
            dvv == Convert.ToString(Query("SELECT Valor FROM DVV WHERE NombreEntidad='Usuario'").Rows[0][0]) && new UsuarioBLL().VerificarIntegridad().Count==0,
            "Repetir la migracion no duplica permisos ni altera usuarios o integridad");
        Assert(traducciones.ObtenerTraduccion(idiomas.First(i => i.Nombre == "English"), "login_title").Texto == "Acceso personalizado de prueba", "La migracion conserva traducciones personalizadas");
        Exec("UPDATE Traduccion SET Texto=N'Perfumery' WHERE NombreEtiqueta='login_title' AND IdIdioma IN (SELECT Id FROM Idioma WHERE Nombre='English')");
        var rol = new SeguridadBLL().ObtenerRolesDisponibles().Single(r => r.Nombre == "Administrador");
        var completo = new SeguridadBLL().ObtenerFamiliaCompleta(rol.Id);
        Assert(completo.TienePermiso(TipoPermiso.Auditoria_Ver) && completo.TienePermiso(TipoPermiso.Auditoria_Restaurar) && completo.TienePermiso(TipoPermiso.Soporte_Gestion),
            "El rol Administrador conserva las funciones administrativas");
    }
    private static void ComprobarSoporte(Usuario owner)
    {
        new UsuarioBLL().Logout(); var soporte = new SoporteBLL();
        Denegado(delegate { new AuditoriaBLL().ListarHistorial(); }, "Sin sesion no se consulta auditoria");
        Denegado(delegate { new AuditoriaBLL().HacerRollback(null); }, "Sin sesion no se restaura auditoria");
        Denegado(delegate { soporte.ListarSolicitudes(); }, "Sin sesion no se listan tickets administrativos");
        Denegado(delegate { soporte.ModificarSolicitudPorAdmin(null); }, "Sin sesion no se administran tickets");
        soporte.CrearSolicitud(new BE.Soporte.SolicitudSoporte { EmailSolicitante="test.soporte@example.com", TipoProblema=soporte.ObtenerTipos().First(), Descripcion="Solicitud de ayuda previa al acceso." });
        Session.Instance.Login(Admin(owner));
        var solicitud = soporte.ListarSolicitudes().Single(s => s.EmailSolicitante == "test.soporte@example.com");
        solicitud.Estado = soporte.ObtenerEstados().Last(); solicitud.ObservacionesAdmin = "Gestion administrativa de prueba.";
        soporte.ModificarSolicitudPorAdmin(solicitud);
        Assert(soporte.ListarSolicitudes().Single(s => s.Id==solicitud.Id).ObservacionesAdmin==solicitud.ObservacionesAdmin, "Administrador puede gestionar el ticket creado sin sesion");
        using (var menu = new FormPrincipal(Session.Instance.Usuario)) {
            menu.Show(); Application.DoEvents();
            Assert(Find<Button>(menu,"btnAuditoria").Visible && Find<Button>(menu,"btnSoporteTecnico").Visible, "Los permisos especificos habilitan ambos menus administrativos");
            menu.Close();
        }
    }
    private static void Exec(string sql) { using (var cmd = new SqlCommand(sql, cn)) cmd.ExecuteNonQuery(); }
    private static void ComprobarFormatoNumerico()
    {
        var anterior = System.Globalization.CultureInfo.CurrentCulture;
        try {
            foreach (string cultura in new[] { "en-US", "es-AR" }) {
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(cultura);
                using (var numero = new NumeroRegional { Maximum=100000, DecimalPlaces=2, ThousandsSeparator=true }) {
                    numero.Value=1234.56m;
                    Assert(numero.Text=="1.234,56", "Editor monetario uniforme con cultura " + cultura);
                    numero.Text="1.234,56"; Assert(numero.Value==1234.56m, "El editor interpreta separadores argentinos con cultura " + cultura);
                    numero.Text="9,50"; numero.UpButton(); Assert(numero.Value==10.50m, "Incrementar conserva el decimal con cultura " + cultura);
                    numero.Text="no numerico"; decimal previo=numero.Value; Assert(previo==10.50m, "Entrada invalida conserva el valor con cultura " + cultura);
                    numero.Maximum=1000; numero.Text="999999"; Assert(numero.Value==1000, "El editor conserva el limite maximo con cultura " + cultura);
                    numero.Minimum=5; numero.Text="-100"; Assert(numero.Value==5, "El editor conserva el limite minimo con cultura " + cultura);
                }
            }
        } finally { System.Globalization.CultureInfo.CurrentCulture=anterior; }
    }
    private static void ComprobarAcceso()
    {
        using (var login = new Login()) {
            login.Show(); Application.DoEvents();
            var correo = Find<TextBox>(login, "txtEmail"); var clave = Find<TextBox>(login, "txtPassword");
            Assert(login.ClientSize.Width <= 700 && Find<Label>(login, "label3").Text == "Perfumería" && !Find<Label>(login, "label4").Text.Contains("edificios"), "Login compacto con identidad de Perfumeria");
            Assert(login.ActiveControl == correo, "El acceso comienza en el correo");
            Click(login, "btnSoporte"); Assert(login.AcceptButton == null, "Enter en Soporte no inicia una sesion accidentalmente");
            Click(login, "btnCancelar"); Assert(login.AcceptButton == Find<Button>(login, "btnIniciarSesion") && login.ActiveControl == correo, "Volver de Soporte recupera el acceso por teclado");
            Click(login, "btnRegistro"); Assert(login.AcceptButton == null, "Enter en Registro no envia el login");
            Click(login, "btnVolver"); Assert(login.AcceptButton == Find<Button>(login, "btnIniciarSesion") && login.ActiveControl == correo, "Volver de Registro recupera el acceso por teclado");
            typeof(Form).GetMethod("ProcessDialogKey", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(login, new object[] { Keys.Tab });
            Assert(login.ActiveControl == clave, "Tab lleva del correo a la contraseña");
            correo.Text = "demo.dueno@perfumeria.example"; clave.Text = "DemoDueno2026!";
            typeof(Form).GetMethod("ProcessDialogKey", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(login, new object[] { Keys.Enter }); Application.DoEvents();
            var menu = Application.OpenForms.OfType<FormPrincipal>().Single();
            Assert(!login.Visible && menu.Visible, "Enter autentica y abre el menu sin pedir aceptar una bienvenida");
            Assert(Find<Label>(menu, "lblSesionActiva").Text.Contains("Compras") && Find<Label>(menu, "lblSesionActiva").Text.Contains(correo.Text), "La sesion de Compras identifica nombre completo, rol y correo");
            ComprobarRestricciones(menu, "Compras");
            menu.Close(); Application.DoEvents();
            Assert(login.Visible && correo.Text == "" && clave.Text == "" && login.ActiveControl == correo, "Cerrar el menu limpia las credenciales y vuelve al correo");
            login.Close();
        }
        foreach (string[] datos in new[] {
            new[] { "contabilidad", "DemoContabilidad2026!" }, new[] { "deposito", "DemoDeposito2026!" }, new[] { "ventas", "DemoVentas2026!" } }) {
            var actor = new UsuarioBLL().Login("demo." + datos[0] + "@perfumeria.example", datos[1]);
            using (var menu = new FormPrincipal(actor)) {
                menu.Show(); menu.ClientSize = new Size(1000, 570); Application.DoEvents();
                var sesion = Find<Label>(menu, "lblSesionActiva"); var cuerpo = Find<Panel>(menu, "panel1");
                Assert(sesion.Text.Contains(actor.NombreCompleto) && actor.Permisos.OfType<Familia>().All(r => sesion.Text.Contains(r.Nombre)) &&
                    menu.ClientRectangle.Contains(sesion.Bounds) && sesion.Bottom <= cuerpo.Top &&
                    TextRenderer.MeasureText(sesion.Text, sesion.Font).Width <= sesion.ClientSize.Width - sesion.Padding.Horizontal,
                    "Actor visible y legible sin superponer la vista: " + datos[0]);
                ComprobarRestricciones(menu, datos[0]);
                menu.Close();
            }
            new UsuarioBLL().Logout();
        }
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
                cn.Open(); ComprobarMigracion(); ComprobarAcceso(); ComprobarSoporte(owner); ComprobarFormatoNumerico(); Session.Instance.Login(owner); int clients = Count("Cliente");
                Assert(FormatoPresentacion.Moneda(60000m).Contains("60.000,00"), "Importes mostrados usan el criterio argentino sin cambiar la cultura del hilo");
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
                Assert(Find<Label>(ventas, "lblTotalesVenta").Text.Contains(FormatoPresentacion.Moneda(cotizacion.Total)), "Total visible antes de confirmar en formato argentino");
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
                Assert(Find<DateTimePicker>(compras,"dtpOrdenDesde").Format==DateTimePickerFormat.Custom && Find<DateTimePicker>(compras,"dtpOrdenDesde").CustomFormat=="dd/MM/yyyy" &&
                    Convert.ToString(orders.Rows[0].Cells["Total"].FormattedValue)==((decimal)orders.Rows[0].Cells["Total"].Value).ToString("N2",FormatoPresentacion.Cultura), "Grilla y fechas de Compras usan la misma convencion regional");
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
                SelectOrder(orders, EstadoOrdenCompra.Aprobada); int emitida = (int)orders.CurrentRow.Cells["IdOrdenCompra"].Value;
                compras.ModoPrueba = false; Click(compras, "btnEmitirOrden"); compras.ModoPrueba = true;
                Assert(Find<Label>(compras, "lblActualizacionCompras").Text == compras.UltimoMensaje && compras.UltimoMensaje.Contains("emitida") &&
                    new AprovisionamientoBLL().ConsultarHistorialCompras(null).Single(o => o.IdOrdenCompra == emitida).Estado == EstadoOrdenCompra.Emitida,
                    "Emitir guarda y muestra el resultado en la vista sin modal de exito");
                int providerId = providers.Items.Cast<Proveedor>().First(p => p.IdProveedor != 0).IdProveedor;
                providers.SelectedValue = providerId;
                string providerName = ((Proveedor)providers.SelectedItem).RazonSocial;
                Assert(orders.Rows.Count > 0 && orders.Rows.Cast<DataGridViewRow>().All(r => (string)r.Cells["Proveedor"].Value == providerName), "Filtro de proveedor usa su nombre");
                Click(compras, "btnActualizarOrdenes"); Assert((int)providers.SelectedValue == providerId, "Actualizar mantiene los filtros disponibles");
                var receipt = Find<ComboBox>(compras, "cmbOrdenRecepcion");
                receipt.SelectedItem = receipt.Items.Cast<object>().First(i => ((OrdenCompra)i.GetType().GetProperty("Orden").GetValue(i, null)).Estado == EstadoOrdenCompra.RecibidaParcial);
                var receiptProduct = Find<ComboBox>(compras, "cmbRecepcionProducto"); receiptProduct.SelectedValue = floral.IdProducto;
                Assert(Find<Label>(compras, "lblSaldoRecepcion").Text.Contains("Pedidas: 10") && Find<Label>(compras, "lblSaldoRecepcion").Text.Contains("Recibidas: 4") &&
                    Find<NumericUpDown>(compras, "nudLoteCantidad").Maximum == 6, "Recepcion muestra proveedor, cantidades y limita el saldo de Floral a seis");
                receiptProduct.SelectedIndex = receiptProduct.Items.Count - 1;
                Assert(Find<NumericUpDown>(compras, "nudLoteCantidad").Maximum == 3, "Cambiar perfume limita al saldo de Madera");
                receipt.SelectedIndex = -1; Assert(!Find<Button>(compras, "btnRegistrarRecepcion").Enabled, "Sin orden pendiente no se habilita recepcion");
                var detailOrder = new AprovisionamientoBLL().ConsultarHistorialCompras(null).First(o => o.Estado == EstadoOrdenCompra.Rechazada);
                using (var detail = new FormDetalleOrdenCompra(detailOrder, "Proveedor de prueba", new ProductoBLL().Listar(false), null)) {
                    Assert(Find<DataGridView>(detail, "dgvDetalleOrden").ReadOnly && Find<DataGridView>(detail, "dgvDetalleOrden").Rows.Count == detailOrder.Detalles.Count &&
                        Find<Label>(detail, "lblRevisionOrden").Text.Contains(detailOrder.MotivoRechazo), "Detalle de solo lectura muestra precios, saldos y motivo de rechazo");
                }
                var suggestedProduct = new Producto { Codigo = "TEST-CONTINUIDAD", Nombre = "Producto de continuidad", PrecioVenta = 100, StockMinimo = 7 };
                new ProductoBLL().Guardar(suggestedProduct);
                int suggestedId = new ProductoBLL().Listar().Single(p => p.Codigo == suggestedProduct.Codigo).IdProducto;
                foreach (var supplier in new ProveedorBLL().Listar(true).Take(2)) {
                    var quote = new CotizacionProveedor { IdProveedor = supplier.IdProveedor, FechaVigencia = DateTime.Today.AddMonths(1) };
                    quote.Detalles.Add(new DetalleCotizacionProveedor { IdProducto = suggestedId, PrecioUnitario = 40, PlazoEntregaDias = 3 }); new AprovisionamientoBLL().RegistrarCotizacion(quote);
                }
                var rootTabs = compras.Controls.OfType<TabControl>().Single(); rootTabs.SelectedTab = rootTabs.TabPages.Cast<TabPage>().Single(t => t.Text == "Evaluación de proveedores");
                var evaluation = (UC_EvaluacionProveedores)Field(compras, "evaluacion"); evaluation.ModoPrueba = true;
                Find<ComboBox>(evaluation, "cmbEvaluacionProducto").SelectedValue = suggestedId; Click(evaluation, "btnCompararProveedores"); Click(evaluation, "btnUsarCantidadSugerida");
                Assert(Find<NumericUpDown>(evaluation, "nudEvaluacionCantidad").Value == 7 && !evaluation.TieneCambiosPendientes, "Usar cantidad sugerida vuelve a comparar sin confirmar ni crear compras");
                string fundamento = "Compra de reposicion fundamentada en stock." + Environment.NewLine + "Se elige por conveniencia y plazo.";
                Find<TextBox>(evaluation, "txtMotivoSeleccion").Text = fundamento;
                Assert(compras.TieneCambiosPendientes, "El fundamento de N03 sin confirmar cuenta como trabajo pendiente");
                evaluation.ModoPrueba = false; Click(evaluation, "btnConfirmarProveedor"); evaluation.ModoPrueba = true;
                Assert(Find<Label>(evaluation, "lblActualizacionEvaluacion").Text == evaluation.UltimoMensaje && evaluation.UltimoMensaje.Contains("Proveedor confirmado"),
                    "Confirmar proveedor muestra el siguiente paso sin modal de exito");
                var confirmada = (EvaluacionProveedor)Field(evaluation,"actual");
                Assert(new EvaluacionProveedorBLL().Obtener(confirmada.IdEvaluacion).MotivoSeleccion==fundamento, "El fundamento de varias lineas se conserva completo al confirmar");
                int ordersBefore = Count("OrdenCompra"); Click(evaluation, "btnOrdenEvaluacion");
                Assert(Find<Button>(evaluation, "btnOrdenEvaluacion").Enabled && Find<Button>(evaluation, "btnOrdenEvaluacion").Text.StartsWith("Abrir orden"), "El borrador vinculado ofrece abrir la orden existente");
                int linked = Convert.ToInt32(Query("SELECT MAX(IdOrdenCompra) FROM OrdenCompra").Rows[0][0]); Click(evaluation, "btnOrdenEvaluacion");
                Assert(rootTabs.SelectedTab.Text == "Órdenes de compra" && (int)orders.CurrentRow.Cells["IdOrdenCompra"].Value == linked && Count("OrdenCompra") == ordersBefore + 1,
                    "Abrir orden navega y selecciona el borrador sin duplicarlo ni mantener filtros que lo oculten");
                using (var menu = new MenuPrueba(owner)) {
                    menu.Show(); Application.DoEvents(); Click(menu, "btnVentas");
                    var saleView = Find<ComboBox>(menu, "cmbProducto"); saleView.SelectedValue = floral.IdProducto; Click(menu, "btnAgregarProducto");
                    int recordedSales = Count("Venta"); Click(menu, "btnAprovisionamiento");
                    Assert(menu.Consultas == 1 && Find<DataGridView>(menu, "dgvVenta").Rows.Count == 1, "Cancelar navegacion conserva el carrito y la vista original");
                    menu.Descartar = true; Click(menu, "btnAprovisionamiento");
                    Assert(menu.Consultas == 2 && !menu.Controls.Find("dgvVenta", true).Any() && Count("Venta") == recordedSales, "Descartar permite navegar sin registrar la venta");
                    Click(menu, "btnVentas"); Assert(menu.Consultas == 2, "Navegar sin datos pendientes no muestra avisos");
                    Find<ComboBox>(menu, "cmbProducto").SelectedValue = floral.IdProducto; Click(menu, "btnAgregarProducto"); menu.Descartar = false; menu.Close();
                    Assert(!menu.IsDisposed && menu.Consultas == 3, "Cancelar cierre conserva la venta en preparacion");
                    menu.Descartar = true; menu.Close();
                }
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
                var lector = (Usuario)owner.Clone(); lector.Permisos = new System.Collections.Generic.List<Componente> { new Patente { PermisoAsociado=TipoPermiso.Auditoria_Ver } };
                Session.Instance.Login(lector);
                using (var lectura = new UC_AuditoriaUsuarios()) {
                    Host(host,lectura); var historial = Find<DataGridView>(lectura,"dgvHistorial");
                    var filaHistorial = historial.Rows.Cast<DataGridViewRow>().First(r => ((UsuarioAuditoria)r.DataBoundItem).Id==target.Id);
                    historial.CurrentCell=filaHistorial.Cells["Email"]; Application.DoEvents();
                    Assert(!Find<Button>(lectura,"btnRollback").Enabled, "Permiso de lectura permite consultar sin habilitar restauracion");
                    Denegado(delegate { new AuditoriaBLL().HacerRollback(version); }, "Lectura no habilita restauracion directa por BLL");
                    Assert(new UsuarioBLL().ListarTodos().Single(u=>u.Id==target.Id).Email==current.Email, "El intento denegado no modifica al usuario");
                }
                Session.Instance.Login(Admin(owner));
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
