using System;
using System.Collections.Generic;
using BE.Interfaces;

namespace BE
{
    public class Usuario : Persona, IEntidadBase, ILogueable, ICloneable
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }

        public int RolId { get; set; }
        public Rol RolAsignado { get; set; }

        public int IntentosFallidos { get; set; }

        public override string TipoEntidad
        {
            get { return "Usuario"; }
        }

        public Usuario()
        {
            Activo = true;
            FechaAlta = DateTime.Now;
            IntentosFallidos = 0;
        }

        public object Clone()
        {
            return (Usuario)this.MemberwiseClone();
        }
    }
}