// Explicit, local-only demo data. Invoked by the installer only when demo data is selected.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BE;
using BE.Seguridad;
using BLL;
using MPP;
using Servicios;

internal static partial class SeedDemoActors
{
    internal sealed class Actor
    {
        public string Key, Name, Password, Dni;
        public string[] Permissions;
        public string Email { get { return "demo." + Key + "@perfumeria.example"; } }
        public string Role { get { return "DEMO - " + Name; } }
    }
    private static readonly Actor[] Actors = {
        new Actor { Key="dueno", Name="Compras", Dni="99000101", Password="DemoDueno2026!", Permissions=new[]{"Aprovisionamiento_Gestion","Inventario_Recepcionar","Ventas_Registrar"} },
        new Actor { Key="contabilidad", Name="Contabilidad", Dni="99000102", Password="DemoContabilidad2026!", Permissions=new[]{"Contabilidad_AprobarCompras"} },
        new Actor { Key="deposito", Name="Deposito", Dni="99000103", Password="DemoDeposito2026!", Permissions=new[]{"Inventario_Recepcionar"} },
        new Actor { Key="ventas", Name="Ventas", Dni="99000104", Password="DemoVentas2026!", Permissions=new[]{"Ventas_Registrar"} }
    };
    private static SqlConnection connection;
    private static SqlTransaction transaction;
    private static int changes;

    private static DataTable Query(string sql, params object[] values)
    {
        using (var command = Command(sql, values))
        using (var adapter = new SqlDataAdapter(command))
        {
            var table = new DataTable(); adapter.Fill(table); return table;
        }
    }
    private static SqlCommand Command(string sql, params object[] values)
    {
        var command = new SqlCommand(sql, connection, transaction);
        for (int i=0; i<values.Length; i++) command.Parameters.AddWithValue("@p"+i, values[i] ?? DBNull.Value);
        return command;
    }
    private static int InsertId(string sql, params object[] values)
    {
        using (var c=Command(sql+"; SELECT CAST(SCOPE_IDENTITY() AS int);",values)) { changes++; return Convert.ToInt32(c.ExecuteScalar()); }
    }
    private static void Execute(string sql, params object[] values)
    {
        using(var c=Command(sql, values)) { c.ExecuteNonQuery(); changes++; }
    }
    private static void Require(bool condition, string message) { if(!condition) throw new InvalidOperationException(message); }
    private static void CheckIntegrity()
    {
        var users=UsuarioMPP.Instancia.MapearLista(Query("SELECT * FROM dbo.Usuarios"));
        var dvv=Query("SELECT Valor FROM dbo.DVV WHERE NombreEntidad='Usuario'");
        var errors=Integridad.VerificarIntegridad(users,dvv.Rows.Count==0 ? null : (string)dvv.Rows[0][0]);
        Require(errors.Count==0,"La base presenta errores de integridad previos. No se recalculan ni se ocultan: revisar desde la aplicacion antes de precargar.");
    }
    private static void InitializeRestoredIntegrity()
    {
        // Installer-only bootstrap of its freshly restored distribution snapshot.
        // Never selected for an existing database by InicializarBaseDatos.ps1.
        var users=UsuarioMPP.Instancia.MapearLista(Query("SELECT * FROM dbo.Usuarios"));
        foreach(var user in users)
        {
            user.DVH=Integridad.CalcularDVH(user);
            Execute("UPDATE dbo.Usuarios SET DVH=@p0 WHERE Id=@p1",user.DVH,user.Id);
        }
        Execute(@"IF EXISTS(SELECT 1 FROM dbo.DVV WHERE NombreEntidad='Usuario') UPDATE dbo.DVV SET Valor=@p0 WHERE NombreEntidad='Usuario'
            ELSE INSERT dbo.DVV(NombreEntidad,Valor) VALUES('Usuario',@p0)",Integridad.CalcularDVV(users));
        Console.WriteLine("INTEGRIDAD INICIALIZADA para el respaldo de distribucion recien restaurado.");
    }
    private static void CheckExisting()
    {
        CheckIntegrity();
        foreach(var actor in Actors)
        {
            var rows=Query("SELECT * FROM dbo.Usuarios WHERE Email=@p0",actor.Email);
            Require(rows.Rows.Count<=1,"Correo duplicado: "+actor.Email);
            if(rows.Rows.Count==1)
            {
                var user=UsuarioMPP.Instancia.MapearUsuario(rows.Rows[0]);
                Require(user.Nombre=="DEMO" && user.Apellido==actor.Name && user.Dni==actor.Dni && user.Password==Encriptador.Hash(actor.Password) && user.Activo && user.IntentosFallidos==0,
                    "La cuenta ya existe y difiere del seed; no se sobrescribira: "+actor.Email);
                var assigned=Query(@"SELECT c.Nombre,c.Tipo FROM dbo.Seguridad_Usuario_Componente u JOIN dbo.Seguridad_Componente c ON c.Id=u.IdComponente WHERE u.IdUsuario=@p0",user.Id);
                Require(assigned.Rows.Count==1 && (string)assigned.Rows[0][0]==actor.Role && (string)assigned.Rows[0][1]=="Familia","Asignaciones existentes diferentes: "+actor.Email);
            }
            else Require(Query("SELECT Id FROM dbo.Usuarios WHERE Dni=@p0",Encriptador.EncriptarAES(actor.Dni)).Rows.Count==0,"DNI de prueba ya utilizado.");
            var roles=Query("SELECT Id FROM dbo.Seguridad_Componente WHERE Nombre=@p0 AND Tipo='Familia'",actor.Role);
            Require(roles.Rows.Count<=1,"Rol demo duplicado: "+actor.Role);
            if(roles.Rows.Count==1)
            {
                var permissions=Query(@"SELECT c.Tipo,c.PermisoAsociado FROM dbo.Seguridad_Familia_Hijo f JOIN dbo.Seguridad_Componente c ON c.Id=f.IdHijo WHERE f.IdPadre=@p0",roles.Rows[0][0]);
                Require(permissions.Rows.Count==actor.Permissions.Length && permissions.Rows.Cast<DataRow>().All(r=>(string)r[0]=="Patente") &&
                    permissions.Rows.Cast<DataRow>().Select(r=>(string)r[1]).OrderBy(x=>x).SequenceEqual(actor.Permissions.OrderBy(x=>x)),"El rol demo existente tiene otros permisos: "+actor.Role);
            }
            foreach(var permission in actor.Permissions)
                Require(Query("SELECT Id FROM dbo.Seguridad_Componente WHERE Tipo='Patente' AND PermisoAsociado=@p0",permission).Rows.Count==1,"Permiso ausente/duplicado; aplicar migraciones antes: "+permission);
        }
        for(int i=1;i<=2;i++)
        {
            var rows=Query("SELECT RazonSocial,Contacto FROM dbo.Proveedor WHERE Cuit=@p0","9900000000"+i);
            Require(rows.Rows.Count==0 || (rows.Rows.Count==1 && (string)rows.Rows[0][0]=="DEMO Proveedor "+i && (string)rows.Rows[0][1]=="demo.proveedor"+i+"@perfumeria.example"),"CUIT de prueba ya utilizado por otro proveedor.");
        }
        var clients=Query("SELECT IdCliente,Nombre,Apellido FROM dbo.Cliente WHERE Email=@p0","demo.cliente@perfumeria.example");
        Require(clients.Rows.Count==0 || (clients.Rows.Count==1 && (string)clients.Rows[0][1]=="Cliente" && (string)clients.Rows[0][2]=="DEMO"),"Correo de cliente demo ya utilizado.");
        var membership=Query("SELECT IdCliente FROM dbo.Membresia WHERE Numero='DEMO-2026-001'");
        Require(membership.Rows.Count==0 || (clients.Rows.Count==1 && membership.Rows.Count==1 && (int)membership.Rows[0][0]==(int)clients.Rows[0][0]),"Numero de membresia demo ya utilizado.");
        CheckScenarioConflicts();
        CheckEvaluationConflicts();
    }
    private static void Apply()
    {
        foreach(var actor in Actors)
        {
            var roles=Query("SELECT Id FROM dbo.Seguridad_Componente WHERE Nombre=@p0 AND Tipo='Familia'",actor.Role);
            int role;
            if(roles.Rows.Count==0)
            {
                role=InsertId("INSERT dbo.Seguridad_Componente(Nombre,Tipo,PermisoAsociado) VALUES(@p0,'Familia',NULL)",actor.Role);
                foreach(var permission in actor.Permissions)
                    Execute(@"INSERT dbo.Seguridad_Familia_Hijo(IdPadre,IdHijo) SELECT @p0,Id FROM dbo.Seguridad_Componente WHERE Tipo='Patente' AND PermisoAsociado=@p1",role,permission);
            }
            else role=(int)roles.Rows[0][0];
            if(Query("SELECT Id FROM dbo.Usuarios WHERE Email=@p0",actor.Email).Rows.Count!=0) continue;
            int id=InsertId(@"INSERT dbo.Usuarios(Dni,Nombre,Apellido,Email,Password,FechaAlta,Activo,IntentosFallidos,DVH)
                VALUES(@p0,'DEMO',@p1,@p2,@p3,GETDATE(),1,0,NULL)",Encriptador.EncriptarAES(actor.Dni),actor.Name,actor.Email,Encriptador.Hash(actor.Password));
            var user=UsuarioMPP.Instancia.MapearUsuario(Query("SELECT * FROM dbo.Usuarios WHERE Id=@p0",id).Rows[0]);
            Execute("UPDATE dbo.Usuarios SET DVH=@p0 WHERE Id=@p1",Integridad.CalcularDVH(user),id);
            Execute("INSERT dbo.Seguridad_Usuario_Componente(IdUsuario,IdComponente) VALUES(@p0,@p1)",id,role);
            Console.WriteLine("CREADO "+actor.Email);
        }
        if(changes>0)
        {
            string dvv=Integridad.CalcularDVV(UsuarioMPP.Instancia.MapearLista(Query("SELECT * FROM dbo.Usuarios")));
            Execute(@"IF EXISTS(SELECT 1 FROM dbo.DVV WHERE NombreEntidad='Usuario') UPDATE dbo.DVV SET Valor=@p0 WHERE NombreEntidad='Usuario'
                ELSE INSERT dbo.DVV(NombreEntidad,Valor) VALUES('Usuario',@p0)",dvv);
        }
        for(int i=1;i<=2;i++)
            if(Query("SELECT IdProveedor FROM dbo.Proveedor WHERE Cuit=@p0","9900000000"+i).Rows.Count==0)
                InsertId(@"INSERT dbo.Proveedor(RazonSocial,Cuit,Contacto,AntiguedadMercado,IndicePrestigio,Activo) VALUES(@p0,@p1,@p2,@p3,@p4,1)","DEMO Proveedor "+i,"9900000000"+i,"demo.proveedor"+i+"@perfumeria.example",i==1?10:5,i==1?90m:80m);
        var clients=Query("SELECT IdCliente FROM dbo.Cliente WHERE Email=@p0","demo.cliente@perfumeria.example");
        int client=clients.Rows.Count==0 ? InsertId(@"INSERT dbo.Cliente(Nombre,Apellido,Email,Telefono,PreferenciasOlfativas,AutorizaMarketing,FechaAlta,Activo)
            VALUES('Cliente','DEMO',@p0,'','Floral',0,GETDATE(),1)","demo.cliente@perfumeria.example") : (int)clients.Rows[0][0];
        if(Query("SELECT IdMembresia FROM dbo.Membresia WHERE IdCliente=@p0",client).Rows.Count==0)
            InsertId("INSERT dbo.Membresia(IdCliente,Numero,FechaAlta,PorcentajeDescuento,Activa) VALUES(@p0,'DEMO-2026-001',GETDATE(),10,1)",client);
        ApplyScenarios();
        ApplyEvaluationScenarios();
        VerifyScenarios();
        VerifyEvaluationScenarios();
        CheckIntegrity();
    }
    private static void VerifyLogin()
    {
        var bll=new UsuarioBLL();
        foreach(var actor in Actors)
        {
            var user=bll.Login(actor.Email,actor.Password);
            Require(user!=null,"Login fallido: "+actor.Email);
            foreach(TipoPermiso permission in Enum.GetValues(typeof(TipoPermiso)))
                Require(user.TienePermiso(permission)==actor.Permissions.Contains(permission.ToString()),"Permiso inesperado: "+actor.Email+" / "+permission);
            bll.Logout();
            Console.WriteLine("LOGIN Y PERMISOS OK "+actor.Email);
        }
        Require(bll.VerificarIntegridad().Count==0,"Integridad incorrecta despues de login.");
    }
    private static int Main(string[] args)
    {
        try
        {
            Require(args.Length==2 && new[]{"--check-target","--check","--apply","--verify","--initialize-new"}.Contains(args[0]),"Uso: SeedDemoActors --check-target|--check|--apply|--verify NombreBase");
            var builder=new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString);
            Require(builder.IntegratedSecurity && builder.InitialCatalog==args[1],"La base no coincide con la indicada o no usa autenticacion integrada.");
            var targetBuilder=new SqlConnectionStringBuilder(builder.ConnectionString);
            if(args[0]=="--check-target") targetBuilder.InitialCatalog="master";
            using(connection=new SqlConnection(targetBuilder.ConnectionString))
            {
                connection.Open();
                var server=Query("SELECT CAST(SERVERPROPERTY('MachineName') AS nvarchar(128)),CAST(SERVERPROPERTY('ProductMajorVersion') AS int),CAST(COALESCE(SERVERPROPERTY('IsLocalDB'),0) AS int)").Rows[0];
                bool isLocalDb=Convert.ToInt32(server[2])==1;
                Require(SeedDemoTargetPolicy.IsLocalTarget(builder.DataSource,Convert.ToString(server[0]),isLocalDb,Environment.MachineName),
                    "Solo se permite una instancia local para datos demo. Instancia="+builder.DataSource+"; SQL MachineName="+Convert.ToString(server[0])+"; equipo="+Environment.MachineName+"; IsLocalDB="+isLocalDb);
                Require((int)server[1]>=16,"Se requiere SQL Server 16.x o superior.");
                Console.WriteLine("DESTINO "+builder.DataSource+" / "+builder.InitialCatalog+"; SQL="+server[1]+"; IsLocalDB="+isLocalDb);
                if(args[0]=="--check-target") { Console.WriteLine("DESTINO LOCAL OK; sin cambios."); return 0; }
                if(args[0]=="--verify") { VerifyLogin(); VerifyScenarios(); VerifyEvaluationScenarios(); return 0; }
                using(transaction=connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    if(args[0]=="--initialize-new") InitializeRestoredIntegrity();
                    CheckExisting();
                    if(args[0]=="--apply" || args[0]=="--initialize-new") { Apply(); transaction.Commit(); }
                    else transaction.Rollback();
                }
                transaction=null;
                Console.WriteLine(args[0]=="--check" ? "PREVALIDACION OK; sin cambios." : "COMMIT OK; operaciones de precarga: "+changes);
                return 0;
            }
        }
        catch(Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }
}
