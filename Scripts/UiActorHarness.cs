using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BE.Seguridad;
using Servicios;
using UI;

internal static class UiActorHarness
{
    [STAThread]
    private static void Main(string[] args)
    {
        string actor = args.Length == 0 ? "dueno" : args[0].ToLowerInvariant();
        Usuario usuario = CrearActor(actor);
        Session.Instance.Login(usuario);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new FormPrincipal(usuario));
    }

    private static Usuario CrearActor(string actor)
    {
        Usuario usuario = new Usuario { Activo = true };
        switch (actor)
        {
            case "contabilidad":
                usuario.Id = 7;
                usuario.Nombre = "Equipo de Contabilidad";
                usuario.Email = "aaa@gmail.com";
                Agregar(usuario, TipoPermiso.Contabilidad_AprobarCompras);
                break;
            case "ventas":
                usuario.Id = 8;
                usuario.Nombre = "Equipo de Ventas";
                usuario.Email = "nile.conde@gmai.com";
                Agregar(usuario, TipoPermiso.Ventas_Registrar);
                break;
            default:
                usuario.Id = 5;
                usuario.Nombre = "Dueño de Perfumería";
                usuario.Email = "mperazzo@gmail.com";
                Agregar(usuario, TipoPermiso.Aprovisionamiento_Gestion);
                Agregar(usuario, TipoPermiso.Inventario_Recepcionar);
                Agregar(usuario, TipoPermiso.Ventas_Registrar);
                break;
        }
        return usuario;
    }

    private static void Agregar(Usuario usuario, TipoPermiso permiso)
    {
        usuario.Permisos.Add(new Patente
        {
            Id = 1000 + (int)permiso,
            Nombre = permiso.ToString(),
            PermisoAsociado = permiso
        });
    }
}
