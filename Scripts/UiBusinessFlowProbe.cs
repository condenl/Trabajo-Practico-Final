using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using BE;
using BE.Seguridad;
using BLL;
using Servicios;
using UI;
using UI.UserControls;

internal static class UiBusinessFlowProbe
{
    private static string sufijo;
    private static string codigoProducto;
    private static string razonProveedorA;
    private static string razonProveedorB;
    private static string emailCliente;

    [STAThread]
    private static int Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            sufijo = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            codigoProducto = "PRUEBAUI-" + sufijo;
            razonProveedorA = "PRUEBA UI Proveedor A " + sufijo;
            razonProveedorB = "PRUEBA UI Proveedor B " + sufijo;
            emailCliente = "prueba.ui." + sufijo + "@example.com";

            Usuario dueno = Actor(5, "Dueño de Perfumería", "mperazzo@gmail.com",
                TipoPermiso.Aprovisionamiento_Gestion, TipoPermiso.Inventario_Recepcionar, TipoPermiso.Ventas_Registrar);
            Usuario contabilidad = Actor(7, "Equipo de Contabilidad", "aaa@gmail.com",
                TipoPermiso.Contabilidad_AprobarCompras);
            Usuario ventas = Actor(8, "Equipo de Ventas", "nile.conde@gmai.com",
                TipoPermiso.Ventas_Registrar);

            VerificarMenusPorActor(dueno, contabilidad, ventas);
            CrearCompraDesdeUi(dueno);
            AprobarDesdeUi(contabilidad);
            RecibirDesdeUi(dueno);
            VenderDesdeUi(ventas);
            VerificarPersistencia();
            VerificarDenegacion(contabilidad);

            Console.WriteLine("RESULTADO=OK");
            Console.WriteLine("PRODUCTO=" + codigoProducto);
            Console.WriteLine("CLIENTE=" + emailCliente);
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("RESULTADO=ERROR");
            Console.Error.WriteLine(error.ToString());
            return 1;
        }
    }

    private static Usuario Actor(int id, string nombre, string email, params TipoPermiso[] permisos)
    {
        Usuario usuario = new Usuario { Id = id, Nombre = nombre, Email = email, Activo = true };
        foreach (TipoPermiso permiso in permisos)
            usuario.Permisos.Add(new Patente { Id = 1000 + (int)permiso, Nombre = permiso.ToString(), PermisoAsociado = permiso });
        return usuario;
    }

    private static void VerificarMenusPorActor(Usuario dueno, Usuario contabilidad, Usuario ventas)
    {
        VerificarMenu(dueno, true, true);
        VerificarMenu(contabilidad, true, false);
        VerificarMenu(ventas, false, true);

        Session.Instance.Login(contabilidad);
        UC_Aprovisionamiento controlContable = new UC_Aprovisionamiento(contabilidad) { ModoPrueba = true };
        TabControl tabs = controlContable.Controls.OfType<TabControl>().Single();
        Exigir(tabs.TabPages.Count == 1 && tabs.TabPages[0].Text == "Aprobación contable",
            "La UI de Contabilidad expuso operaciones ajenas a su perfil.");
        controlContable.Dispose();
    }

    private static void VerificarMenu(Usuario actor, bool comprasVisible, bool ventasVisible)
    {
        Session.Instance.Login(actor);
        using (FormPrincipal form = new FormPrincipal(actor))
        {
            form.Show();
            Application.DoEvents();
            Exigir(Encontrar<Button>(form, "btnAprovisionamiento").Visible == comprasVisible,
                "Visibilidad incorrecta del menú Compras para " + actor.Nombre + ".");
            Exigir(Encontrar<Button>(form, "btnVentas").Visible == ventasVisible,
                "Visibilidad incorrecta del menú Ventas para " + actor.Nombre + ".");
            form.Close();
        }
    }

    private static void CrearCompraDesdeUi(Usuario dueno)
    {
        Session.Instance.Login(dueno);
        using (UC_Aprovisionamiento ui = new UC_Aprovisionamiento(dueno) { ModoPrueba = true })
        using (Form host = Hospedar(ui))
        {
            GuardarProveedor(ui, razonProveedorA, "30" + sufijo.Substring(sufijo.Length - 9));
            GuardarProveedor(ui, razonProveedorB, "31" + sufijo.Substring(sufijo.Length - 9));

            Encontrar<TextBox>(ui, "txtProductoCodigo").Text = codigoProducto;
            Encontrar<TextBox>(ui, "txtProductoNombre").Text = "Perfume prueba UI";
            Encontrar<TextBox>(ui, "txtProductoMarca").Text = "Marca E2E";
            Encontrar<TextBox>(ui, "txtProductoFamilia").Text = "Cítrica";
            Encontrar<NumericUpDown>(ui, "nudProductoPrecio").Value = 250m;
            Encontrar<NumericUpDown>(ui, "nudProductoStockMinimo").Value = 1;
            Pulsar(ui, "btnGuardarProducto");
            SinError(ui, "guardar el producto");

            RegistrarCotizacion(ui, razonProveedorA, 100m);
            RegistrarCotizacion(ui, razonProveedorB, 110m);

            SeleccionarPestana(ui, "Órdenes de compra");
            Seleccionar<Proveedor>(Encontrar<ComboBox>(ui, "cmbOrdenProveedor"), p => p.RazonSocial == razonProveedorA);
            Seleccionar<Producto>(Encontrar<ComboBox>(ui, "cmbOrdenProducto"), p => p.Codigo == codigoProducto);
            Encontrar<NumericUpDown>(ui, "nudOrdenCantidad").Value = 3;
            Encontrar<NumericUpDown>(ui, "nudOrdenPrecio").Value = 100m;
            Pulsar(ui, "btnCrearOrden");
            SinError(ui, "crear la orden");
            SeleccionarPrimeraFila(Encontrar<DataGridView>(ui, "dgvOrdenes"));
            Pulsar(ui, "btnEnviarRevision");
            SinError(ui, "enviar la orden a Contabilidad");
        }
    }

    private static void GuardarProveedor(UC_Aprovisionamiento ui, string razon, string cuit)
    {
        Encontrar<TextBox>(ui, "txtProveedorRazon").Text = razon;
        Encontrar<TextBox>(ui, "txtProveedorCuit").Text = cuit;
        Encontrar<TextBox>(ui, "txtProveedorContacto").Text = "contacto@example.com";
        Encontrar<NumericUpDown>(ui, "nudProveedorAntiguedad").Value = 10;
        Encontrar<NumericUpDown>(ui, "nudProveedorPrestigio").Value = 90;
        Pulsar(ui, "btnGuardarProveedor");
        SinError(ui, "guardar el proveedor");
    }

    private static void RegistrarCotizacion(UC_Aprovisionamiento ui, string razon, decimal precio)
    {
        Seleccionar<Proveedor>(Encontrar<ComboBox>(ui, "cmbCotizacionProveedor"), p => p.RazonSocial == razon);
        Seleccionar<Producto>(Encontrar<ComboBox>(ui, "cmbCotizacionProducto"), p => p.Codigo == codigoProducto);
        Encontrar<NumericUpDown>(ui, "nudCotizacionPrecio").Value = precio;
        Encontrar<NumericUpDown>(ui, "nudCotizacionPlazo").Value = 5;
        Encontrar<TextBox>(ui, "txtCotizacionCondiciones").Text = "Cotización de prueba E2E";
        Pulsar(ui, "btnRegistrarCotizacion");
        SinError(ui, "registrar la cotización");
    }

    private static void AprobarDesdeUi(Usuario contabilidad)
    {
        Session.Instance.Login(contabilidad);
        using (UC_Aprovisionamiento ui = new UC_Aprovisionamiento(contabilidad) { ModoPrueba = true })
        using (Form host = Hospedar(ui))
        {
            DataGridView pendientes = Encontrar<DataGridView>(ui, "dgvPendientes");
            Exigir(pendientes.Rows.Count > 0, "Contabilidad no recibió la orden pendiente.");
            SeleccionarPrimeraFila(pendientes);
            Pulsar(ui, "btnAprobarOrden");
            SinError(ui, "aprobar la orden");
        }
    }

    private static void RecibirDesdeUi(Usuario dueno)
    {
        Session.Instance.Login(dueno);
        using (UC_Aprovisionamiento ui = new UC_Aprovisionamiento(dueno) { ModoPrueba = true })
        using (Form host = Hospedar(ui))
        {
            SeleccionarPestana(ui, "Órdenes de compra");
            SeleccionarPrimeraFila(Encontrar<DataGridView>(ui, "dgvOrdenes"));
            Pulsar(ui, "btnEmitirOrden");
            SinError(ui, "emitir la orden aprobada");

            SeleccionarPestana(ui, "Recepción e inventario");
            ComboBox ordenes = Encontrar<ComboBox>(ui, "cmbOrdenRecepcion");
            Exigir(ordenes.Items.Count > 0, "La orden emitida no apareció para recepción.");
            ordenes.SelectedIndex = 0;
            ComboBox productos = Encontrar<ComboBox>(ui, "cmbRecepcionProducto");
            Seleccionar<Producto>(productos, p => p.Codigo == codigoProducto);
            Encontrar<TextBox>(ui, "txtLoteCodigo").Text = "LOTE-" + sufijo;
            Encontrar<TextBox>(ui, "txtLoteOrigen").Text = "Industria Argentina";
            Encontrar<TextBox>(ui, "txtLoteCertificado").Text = "CERT-" + sufijo;
            Encontrar<NumericUpDown>(ui, "nudLoteCantidad").Value = 3;
            Pulsar(ui, "btnRegistrarRecepcion");
            SinError(ui, "registrar la recepción");
        }
    }

    private static void VenderDesdeUi(Usuario ventas)
    {
        Session.Instance.Login(ventas);
        using (UC_Ventas ui = new UC_Ventas { ModoPrueba = true })
        using (Form host = Hospedar(ui))
        {
            Encontrar<TextBox>(ui, "txtEmail").Text = emailCliente;
            Encontrar<TextBox>(ui, "txtNombre").Text = "Cliente";
            Encontrar<TextBox>(ui, "txtApellido").Text = "Prueba UI";
            Encontrar<TextBox>(ui, "txtTelefono").Text = "11111111";
            Encontrar<TextBox>(ui, "txtPreferencias").Text = "Cítricos";
            Encontrar<CheckBox>(ui, "chkMarketing").Checked = true;
            Encontrar<NumericUpDown>(ui, "nudDescuento").Value = 10m;
            PulsarPorTexto(ui, "Registrar cliente");
            SinError(ui, "registrar al cliente");

            Seleccionar<Producto>(Encontrar<ComboBox>(ui, "cmbProducto"), p => p.Codigo == codigoProducto);
            Encontrar<NumericUpDown>(ui, "nudCantidad").Value = 1;
            PulsarPorTexto(ui, "Agregar producto");
            PulsarPorTexto(ui, "Confirmar venta");
            SinError(ui, "registrar la venta");
        }
    }

    private static void VerificarPersistencia()
    {
        string cs = ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString;
        using (SqlConnection cn = new SqlConnection(cs))
        using (SqlCommand cmd = cn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT COUNT(*)
FROM Producto p
JOIN DetalleOrdenCompra d ON d.IdProducto = p.IdProducto
JOIN OrdenCompra o ON o.IdOrdenCompra = d.IdOrdenCompra
JOIN Lote l ON l.IdOrdenCompra = o.IdOrdenCompra AND l.IdProducto = p.IdProducto
JOIN DetalleVenta dv ON dv.IdLote = l.IdLote
JOIN Venta v ON v.IdVenta = dv.IdVenta
JOIN Cliente c ON c.IdCliente = v.IdCliente
JOIN Membresia m ON m.IdCliente = c.IdCliente AND m.Activa = 1
WHERE p.Codigo = @Codigo
  AND c.Email = @Email
  AND o.Estado = 6
  AND o.IdUsuarioAprobadorContable = 7
  AND l.CantidadIngresada = 3
  AND l.CantidadDisponible = 2
  AND v.Subtotal = 250
  AND v.Descuento = 25
  AND v.Total = 225;";
            cmd.Parameters.AddWithValue("@Codigo", codigoProducto);
            cmd.Parameters.AddWithValue("@Email", emailCliente);
            cn.Open();
            Exigir(Convert.ToInt32(cmd.ExecuteScalar()) == 1,
                "La persistencia final no refleja compra aprobada, trazabilidad, membresía, descuento y salida de stock.");
        }
    }

    private static void VerificarDenegacion(Usuario contabilidad)
    {
        Session.Instance.Login(contabilidad);
        try
        {
            new AprovisionamientoBLL().CrearOrdenPreliminar(new OrdenCompra(), new System.Collections.Generic.List<DetalleOrdenCompra>());
            throw new Exception("Contabilidad pudo iniciar una compra sin permiso.");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("AUTORIZACION=OK");
        }
    }

    private static void SeleccionarPrimeraFila(DataGridView grilla)
    {
        Exigir(grilla.Rows.Count > 0, "La grilla no contiene registros para seleccionar.");
        grilla.ClearSelection();
        grilla.Rows[0].Selected = true;
        grilla.CurrentCell = grilla.Rows[0].Cells.Cast<DataGridViewCell>().First(c => c.Visible);
    }

    private static Form Hospedar(Control control)
    {
        Form host = new Form
        {
            Text = "Prueba UI automatizada - " + control.GetType().Name,
            Width = 1400,
            Height = 760,
            StartPosition = FormStartPosition.Manual,
            Left = 20,
            Top = 20
        };
        control.Dock = DockStyle.Fill;
        host.Controls.Add(control);
        host.Show();
        Application.DoEvents();
        return host;
    }

    private static void SeleccionarPestana(UC_Aprovisionamiento ui, string texto)
    {
        TabControl tabs = ui.Controls.OfType<TabControl>().Single();
        TabPage pagina = tabs.TabPages.Cast<TabPage>().FirstOrDefault(p => p.Text == texto);
        if (pagina == null) throw new Exception("No se encontró la pestaña '" + texto + "'.");
        tabs.SelectedTab = pagina;
        Application.DoEvents();
    }

    private static void Seleccionar<T>(ComboBox combo, Func<T, bool> condicion) where T : class
    {
        for (int i = 0; i < combo.Items.Count; i++)
        {
            T item = combo.Items[i] as T;
            if (item != null && condicion(item))
            {
                combo.SelectedIndex = i;
                return;
            }
        }
        throw new Exception("No se encontró el elemento solicitado en " + combo.Name + ".");
    }

    private static void Pulsar(Control raiz, string nombre)
    {
        Encontrar<Button>(raiz, nombre).PerformClick();
        Application.DoEvents();
    }

    private static void PulsarPorTexto(Control raiz, string texto)
    {
        Button boton = Todos(raiz).OfType<Button>().FirstOrDefault(b => b.Text == texto);
        if (boton == null) throw new Exception("No se encontró el botón '" + texto + "'.");
        boton.PerformClick();
        Application.DoEvents();
    }

    private static T Encontrar<T>(Control raiz, string nombre) where T : Control
    {
        T control = Todos(raiz).OfType<T>().FirstOrDefault(c => c.Name == nombre);
        if (control == null) throw new Exception("No se encontró el control " + nombre + ".");
        return control;
    }

    private static System.Collections.Generic.IEnumerable<Control> Todos(Control raiz)
    {
        foreach (Control hijo in raiz.Controls)
        {
            yield return hijo;
            foreach (Control descendiente in Todos(hijo)) yield return descendiente;
        }
    }

    private static void SinError(UC_Aprovisionamiento ui, string operacion)
    {
        if (!string.IsNullOrWhiteSpace(ui.UltimoError)) throw new Exception("Error al " + operacion + ": " + ui.UltimoError);
    }

    private static void SinError(UC_Ventas ui, string operacion)
    {
        if (!string.IsNullOrWhiteSpace(ui.UltimoError)) throw new Exception("Error al " + operacion + ": " + ui.UltimoError);
    }

    private static void Exigir(bool condicion, string mensaje)
    {
        if (!condicion) throw new Exception(mensaje);
    }
}
