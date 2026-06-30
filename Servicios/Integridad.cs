using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BE;

namespace Servicios
{
    public static class Integridad
    {
        public static string CalcularDVH(Usuario u)
        {
            StringBuilder sb = new StringBuilder();
            // Atributos sensibles para el cálculo de integridad
            string[] atributos = { 
                u.Id.ToString(), 
                u.Email, 
                u.Password, 
                u.Dni, 
                u.Activo.ToString(), 
                u.IntentosFallidos.ToString(), 
                u.Nombre, 
                u.Apellido, 
                u.FechaAlta.ToString() 
            };

            for (int i = 0; i < atributos.Length; i++)
            {
                string valorCampo = atributos[i] ?? "";
                sb.Append(valorCampo + i.ToString());
            }
            return Encriptador.Hash(sb.ToString());
        }

        public static string CalcularDVV(List<Usuario> usuarios)
        {
            StringBuilder sb = new StringBuilder();
            var usuariosOrdenados = usuarios.OrderBy(u => u.Id).ToList();
            foreach (var u in usuariosOrdenados)
            {
                sb.Append(u.DVH);
            }
            return Encriptador.Hash(sb.ToString());
        }

        public static List<string> VerificarIntegridad(List<Usuario> usuarios, string storedDVV)
        {
            List<string> errores = new List<string>();

            // valido DVHs
            foreach (var u in usuarios)
            {
                string dvhCalculado = CalcularDVH(u);
                if (u.DVH != dvhCalculado)
                {
                    errores.Add($"Error de integridad (DVH) en Usuario ID: {u.Id} - Datos del registro: [Nombre: {u.Nombre}, Apellido: {u.Apellido}, Email: {u.Email}, DNI: {u.Dni}, Activo: {u.Activo}, Intentos Fallidos: {u.IntentosFallidos}, Fecha Alta: {u.FechaAlta:yyyy-MM-dd HH:mm:ss}]");
                }
            }

            // valido DVV
            string dvvCalculado = CalcularDVV(usuarios);

            if (usuarios.Count() > 0 && storedDVV != dvvCalculado)
            {
                errores.Add("Error de integridad (DVV) en la tabla Usuario. Se detectaron cambios en la cantidad o posición de los registros.");
            }
            return errores;
        }
    }
}
