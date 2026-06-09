using System;
using System.Collections.Generic;
using BE.Interfaces;
using BE.Seguridad;

namespace BE
{
    public class Usuario : Persona, IEntidadBase, ILogueable, ICloneable
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }
        public int IntentosFallidos { get; set; }
        public string DVH { get; set; }

        public List<Componente> Permisos { get; set; }

        public override string TipoEntidad
        {
            get { return "Usuario"; }
        }

        public Usuario()
        {
            Activo = true;
            FechaAlta = DateTime.Now;
            IntentosFallidos = 0;
            Permisos = new List<Componente>();
        }

        public bool TienePermiso(TipoPermiso permisoBuscado)
        {
            foreach (var componente in Permisos)
            {
                if (ExistePermisoRecursivo(componente, permisoBuscado))
                    return true;
            }
            return false;
        }

        private bool ExistePermisoRecursivo(Componente componente, TipoPermiso permisoBuscado)
        {
            if (componente.PermisoAsociado == permisoBuscado)
                return true;

            foreach (var hijo in componente.ObtenerHijos())
            {
                if (ExistePermisoRecursivo(hijo, permisoBuscado))
                    return true;
            }

            return false;
        }

        public object Clone()
        {
            return (Usuario)this.MemberwiseClone();
        }
    }
}