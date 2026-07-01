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

        // --- 1. EL MOTOR PRIVADO (Reutilizable) ---
        private Dictionary<int, Componente> ConstruirDiccionario(DataTable dtComponentes, DataTable dtRelaciones)
        {
            var diccionario = new Dictionary<int, Componente>();

            // Paso A: Crear todos los objetos sueltos
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

            // Paso B: Vincular Padres con Hijos en memoria (Armar el Grafo)
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

        // --- 2. SALIDA PARA EL USUARIO (Login) ---
        public List<Componente> ConstruirArbolUsuario(DataTable dtComponentes, DataTable dtRelaciones, DataTable dtAsignacionesUsuario)
        {
            // Llamamos al motor privado
            var diccionario = ConstruirDiccionario(dtComponentes, dtRelaciones);

            List<Componente> arbolUsuario = new List<Componente>();

            // Filtramos solo las raíces que tiene asignadas el usuario
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

        // --- 3. SALIDA PARA UN ROL ESPECÍFICO (ABM Roles) ---
        public Familia ConstruirFamilia(DataTable dtComponentes, DataTable dtRelaciones, int idFamilia)
        {
            // Llamamos al mismo motor privado (Reutilización total)
            var diccionario = ConstruirDiccionario(dtComponentes, dtRelaciones);

            // Buscamos el rol solicitado y lo devolvemos con todos sus hijos ya enganchados
            if (diccionario.ContainsKey(idFamilia) && diccionario[idFamilia] is Familia familiaEncontrada)
            {
                return familiaEncontrada;
            }

            return null;
        }
    }
}