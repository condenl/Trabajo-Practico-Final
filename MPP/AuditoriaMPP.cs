using System;
using System.Collections.Generic;
using System.Data;
using BE;
using Servicios;

namespace MPP
{
    public class AuditoriaMPP
    {
        private static AuditoriaMPP instancia;
        private static readonly object bloqueo = new object();

        private AuditoriaMPP() { }

        public static AuditoriaMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new AuditoriaMPP();
                    return instancia;
                }
            }
        }

        public UsuarioAuditoria MapearAuditoria(DataRow fila)
        {
            try
            {
                UsuarioAuditoria audit = new UsuarioAuditoria();

                audit.AuditId = Convert.ToInt32(fila["AuditId"]);
                audit.AuditDate = Convert.ToDateTime(fila["AuditDate"]);
                audit.AuditAction = fila["AuditAction"].ToString();
                audit.UsuarioResponsable = fila["UsuarioResponsable"] != DBNull.Value ? fila["UsuarioResponsable"].ToString() : "SISTEMA";
                audit.Id = Convert.ToInt32(fila["Id"]);

                string dniCifrado = fila["Dni"].ToString();
                audit.Dni = Encriptador.DesencriptarAES(dniCifrado);

                audit.Nombre = fila["Nombre"].ToString();
                audit.Apellido = fila["Apellido"].ToString();
                audit.Email = fila["Email"].ToString();
                audit.Password = fila["Password"].ToString();
                audit.FechaAlta = Convert.ToDateTime(fila["FechaAlta"]);
                audit.Activo = Convert.ToBoolean(fila["Activo"]);

                if (fila.Table.Columns.Contains("IntentosFallidos") && fila["IntentosFallidos"] != DBNull.Value)
                    audit.IntentosFallidos = Convert.ToInt32(fila["IntentosFallidos"]);

                if (fila.Table.Columns.Contains("DVH") && fila["DVH"] != DBNull.Value)
                    audit.DVH = fila["DVH"].ToString();

                return audit;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al mapear auditoría.", ex);
            }
        }

        public List<UsuarioAuditoria> MapearLista(DataTable dt)
        {
            List<UsuarioAuditoria> lista = new List<UsuarioAuditoria>();
            if (dt != null)
            {
                foreach (DataRow fila in dt.Rows)
                    lista.Add(MapearAuditoria(fila));
            }
            return lista;
        }
    }
}