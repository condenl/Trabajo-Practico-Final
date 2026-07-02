using System;
using System.Collections.Generic;
using System.Data;
using BE.Seguridad;

namespace MPP
{
    public class SeguridadMPP
    {
        private static SeguridadMPP instancia;
        private static readonly object bloqueo = new object();

        private SeguridadMPP() { }

        public static SeguridadMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new SeguridadMPP();
                    return instancia;
                }
            }
        }
        private Dictionary<int, Componente> ConstruirDiccionario(DataTable dtComponentes, DataTable dtRelaciones)
        {
            var diccionario = new Dictionary<int, Componente>();

            foreach (DataRow row in dtComponentes.Rows)
            {
                Componente c;
                string tipo = row["Tipo"].ToString();

                if (tipo == "Familia")
                {
                    c = new Familia();
                }
                else
                {
                    c = new Patente();
                    if (row["PermisoAsociado"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["PermisoAsociado"].ToString()))
                    {
                        c.PermisoAsociado = (TipoPermiso)Enum.Parse(typeof(TipoPermiso), row["PermisoAsociado"].ToString());
                    }
                }

                c.Id = Convert.ToInt32(row["Id"]);
                c.Nombre = row["Nombre"].ToString();
                diccionario.Add(c.Id, c);
            }

            foreach (DataRow row in dtRelaciones.Rows)
            {
                int idPadre = Convert.ToInt32(row["IdPadre"]);
                int idHijo = Convert.ToInt32(row["IdHijo"]);

                if (diccionario.ContainsKey(idPadre) && diccionario.ContainsKey(idHijo))
                {
                    diccionario[idPadre].AgregarHijo(diccionario[idHijo]);
                }
            }

            return diccionario;
        }

        public List<Componente> ConstruirArbolUsuario(DataTable dtComponentes, DataTable dtRelaciones, DataTable dtAsignacionesUsuario)
        {
            var diccionario = ConstruirDiccionario(dtComponentes, dtRelaciones);

            List<Componente> arbolUsuario = new List<Componente>();

            foreach (DataRow row in dtAsignacionesUsuario.Rows)
            {
                int idComponente = Convert.ToInt32(row["IdComponente"]);
                if (diccionario.ContainsKey(idComponente))
                {
                    arbolUsuario.Add(diccionario[idComponente]);
                }
            }

            return arbolUsuario;
        }

        public Familia ConstruirFamilia(DataTable dtComponentes, DataTable dtRelaciones, int idFamilia)
        {
            var diccionario = ConstruirDiccionario(dtComponentes, dtRelaciones);

            if (diccionario.ContainsKey(idFamilia) && diccionario[idFamilia] is Familia familiaEncontrada)
            {
                return familiaEncontrada;
            }

            return null;
        }
    }
}